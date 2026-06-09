using System.Diagnostics;
using System.Reflection;
using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using Nino.Core;

namespace SerializationBench;

[MessagePackObject]
[NinoType]
public partial class DemoPingRequest
{
    [Key(0)]
    public string Message { get; set; } = string.Empty;
}

[MessagePackObject]
[NinoType]
public partial class DemoPingResponse
{
    [Key(0)]
    public bool Success { get; set; }

    [Key(1)]
    public string Reply { get; set; } = string.Empty;

    [Key(2)]
    public long ServerTicksUtc { get; set; }
}

[MessagePackObject]
[NinoType]
public partial class UserLogin
{
    [Key(0)]
    public string Username { get; set; } = string.Empty;

    [Key(1)]
    public string Password { get; set; } = string.Empty;
}

[MessagePackObject]
[NinoType]
public partial class UserLoginResponse
{
    [Key(0)]
    public bool Success { get; set; }

    [Key(1)]
    public string Message { get; set; } = string.Empty;

    [Key(2)]
    public string Token { get; set; } = string.Empty;
}

internal static class Program
{
    private const int Iterations = 200_000;

    private static readonly MessagePackSerializerOptions MessagePackStandardOptions =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    private static readonly MessagePackSerializerOptions MessagePackSourceGeneratedOptions =
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
                SourceGeneratedFormatterResolver.Instance,
                BuiltinResolver.Instance,
                AttributeFormatterResolver.Instance))
            .WithCompression(MessagePackCompression.Lz4BlockArray);

    private static int Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "all";

        switch (mode)
        {
            case "all":
                RunAll();
                return 0;
            case "messagepack":
            case "msgpack":
            case "mp":
                RunMessagePackOnly();
                return 0;
            case "messagepack-sg":
            case "msgpack-sg":
            case "mp-sg":
                RunMessagePackSourceGeneratedOnly();
                return 0;
            case "messagepack-noalloc":
            case "msgpack-noalloc":
            case "mp-noalloc":
                RunMessagePackNoAllocOnly();
                return 0;
            case "nino":
                RunNinoOnly();
                return 0;
            default:
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run --project .\\SerializationBench\\SerializationBench.csproj");
                Console.WriteLine("  dotnet run --project .\\SerializationBench\\SerializationBench.csproj -c Release -- messagepack");
                Console.WriteLine("  dotnet run --project .\\SerializationBench\\SerializationBench.csproj -c Release -- messagepack-sg");
                Console.WriteLine("  dotnet run --project .\\SerializationBench\\SerializationBench.csproj -c Release -- messagepack-noalloc");
                Console.WriteLine("  dotnet run --project .\\SerializationBench\\SerializationBench.csproj -c Release -- nino");
                return 1;
        }
    }

    private static void RunAll()
    {
        Console.WriteLine("=== Serialization Benchmark ===");
        Console.WriteLine($".NET: {Environment.Version}");
        Console.WriteLine($"Build: {GetBuildConfiguration()}");
        Console.WriteLine($"Iterations: {Iterations}");
        Console.WriteLine($"TieredCompilation: {GetRuntimeSwitch("System.Runtime.TieredCompilation")}");
        Console.WriteLine($"TieredPGO: {GetRuntimeSwitch("System.Runtime.TieredPGO")}");
        Console.WriteLine($"MessagePack standard resolver: {MessagePackStandardOptions.Resolver.GetType().FullName}");
        Console.WriteLine($"MessagePack SG-priority resolver: {MessagePackSourceGeneratedOptions.Resolver.GetType().FullName}");
        Console.WriteLine("MessagePack compression: Lz4BlockArray");
        Console.WriteLine();

        BenchmarkPair(
            "DemoPingRequest",
            new DemoPingRequest { Message = "hello-from-benchmark" });

        BenchmarkPair(
            "DemoPingResponse",
            new DemoPingResponse
            {
                Success = true,
                Reply = "PONG: hello-from-benchmark",
                ServerTicksUtc = DateTime.UtcNow.Ticks
            });

        BenchmarkPair(
            "UserLogin",
            new UserLogin
            {
                Username = "demo_user",
                Password = "super-secret-password"
            });

        BenchmarkPair(
            "UserLoginResponse",
            new UserLoginResponse
            {
                Success = true,
                Message = "login ok",
                Token = "ey.mock.jwt.token"
            });
    }

    private static void RunMessagePackOnly()
    {
        Console.WriteLine("=== MessagePack Standard Sanity Check ===");
        var payload = new DemoPingRequest { Message = "messagepack-only" };
        byte[] bytes = MessagePackStandardSerialize(payload);
        var roundTrip = MessagePackStandardDeserialize<DemoPingRequest>(bytes);
        Console.WriteLine($"Payload bytes: {bytes.Length}");
        Console.WriteLine($"Round-trip message: {roundTrip.Message}");
    }

    private static void RunMessagePackSourceGeneratedOnly()
    {
        Console.WriteLine("=== MessagePack Source Generator Priority Sanity Check ===");
        var payload = new DemoPingRequest { Message = "messagepack-sg-only" };
        byte[] bytes = MessagePackSourceGeneratedSerialize(payload);
        var roundTrip = MessagePackSourceGeneratedDeserialize<DemoPingRequest>(bytes);
        Console.WriteLine($"Resolver: {MessagePackSourceGeneratedOptions.Resolver.GetType().FullName}");
        Console.WriteLine($"Payload bytes: {bytes.Length}");
        Console.WriteLine($"Round-trip message: {roundTrip.Message}");
    }

    private static void RunMessagePackNoAllocOnly()
    {
        Console.WriteLine("=== MessagePack No-Alloc Serialize Sanity Check ===");
        var payload = new DemoPingRequest { Message = "messagepack-noalloc-only" };
        ArrayBufferWriter<byte> bufferWriter = new();
        MessagePackSourceGeneratedSerializeToBuffer(payload, bufferWriter);
        byte[] bytes = bufferWriter.WrittenSpan.ToArray();
        var roundTrip = MessagePackSourceGeneratedDeserialize<DemoPingRequest>(bytes);
        Console.WriteLine($"Resolver: {MessagePackSourceGeneratedOptions.Resolver.GetType().FullName}");
        Console.WriteLine($"Payload bytes: {bytes.Length}");
        Console.WriteLine($"Round-trip message: {roundTrip.Message}");
    }

    private static void RunNinoOnly()
    {
        Console.WriteLine("=== Nino Sanity Check ===");
        var payload = new DemoPingRequest { Message = "nino-only" };
        byte[] bytes = NinoSerialize(payload);
        var roundTrip = NinoDeserialize<DemoPingRequest>(bytes);
        Console.WriteLine($"Payload bytes: {bytes.Length}");
        Console.WriteLine($"Round-trip message: {roundTrip.Message}");
    }

    private static void BenchmarkPair<T>(
        string name,
        T payload)
    {
        byte[] messagePackStandardBytes = MessagePackStandardSerialize(payload);
        byte[] messagePackSourceGeneratedBytes = MessagePackSourceGeneratedSerialize(payload);
        byte[] ninoBytes = NinoSerialize(payload);

        T messagePackStandardRoundTrip = MessagePackStandardDeserialize<T>(messagePackStandardBytes);
        T messagePackSourceGeneratedRoundTrip = MessagePackSourceGeneratedDeserialize<T>(messagePackSourceGeneratedBytes);
        T ninoRoundTrip = NinoDeserialize<T>(ninoBytes);

        Console.WriteLine($"[{name}]");
        Console.WriteLine($"  MessagePack standard bytes: {messagePackStandardBytes.Length}");
        Console.WriteLine($"  MessagePack SG-priority bytes: {messagePackSourceGeneratedBytes.Length}");
        Console.WriteLine($"  Nino bytes:                 {ninoBytes.Length}");
        Console.WriteLine($"  MessagePack standard serialize alloc: {MeasureSerialize(payload, MessagePackStandardSerialize):N0} ops/s");
        Console.WriteLine($"  MessagePack standard deserialize:     {MeasureDeserialize(messagePackStandardBytes, MessagePackStandardDeserialize<T>):N0} ops/s");
        Console.WriteLine($"  MessagePack SG-priority serialize alloc:  {MeasureSerialize(payload, MessagePackSourceGeneratedSerialize):N0} ops/s");
        Console.WriteLine($"  MessagePack SG-priority deserialize:      {MeasureDeserialize(messagePackSourceGeneratedBytes, MessagePackSourceGeneratedDeserialize<T>):N0} ops/s");
        Console.WriteLine($"  MessagePack SG-priority serialize noalloc: {MeasureMessagePackSerializeNoAlloc(payload):N0} ops/s");
        Console.WriteLine($"  Nino serialize alloc:                 {MeasureSerialize(payload, NinoSerialize):N0} ops/s");
        Console.WriteLine($"  Nino deserialize:                     {MeasureDeserialize(ninoBytes, NinoDeserialize<T>):N0} ops/s");
        Console.WriteLine($"  Round-trip OK: MP-Std={messagePackStandardRoundTrip is not null}, MP-SG={messagePackSourceGeneratedRoundTrip is not null}, Nino={ninoRoundTrip is not null}");
        Console.WriteLine();
    }

    private static double MeasureSerialize<T>(T payload, Func<T, byte[]> serialize)
    {
        serialize(payload);

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            serialize(payload);
        }

        stopwatch.Stop();
        return Iterations / stopwatch.Elapsed.TotalSeconds;
    }

    private static double MeasureDeserialize<T>(byte[] bytes, Func<byte[], T> deserialize)
    {
        deserialize(bytes);

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            deserialize(bytes);
        }

        stopwatch.Stop();
        return Iterations / stopwatch.Elapsed.TotalSeconds;
    }

    private static double MeasureMessagePackSerializeNoAlloc<T>(T payload)
    {
        ArrayBufferWriter<byte> writer = new();
        MessagePackSourceGeneratedSerializeToBuffer(payload, writer);

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            MessagePackSourceGeneratedSerializeToBuffer(payload, writer);
        }

        stopwatch.Stop();
        return Iterations / stopwatch.Elapsed.TotalSeconds;
    }

    private static byte[] MessagePackStandardSerialize<T>(T value)
    {
        return MessagePackSerializer.Serialize(value, MessagePackStandardOptions);
    }

    private static T MessagePackStandardDeserialize<T>(byte[] bytes)
    {
        return MessagePackSerializer.Deserialize<T>(bytes, MessagePackStandardOptions);
    }

    private static byte[] MessagePackSourceGeneratedSerialize<T>(T value)
    {
        return MessagePackSerializer.Serialize(value, MessagePackSourceGeneratedOptions);
    }

    private static T MessagePackSourceGeneratedDeserialize<T>(byte[] bytes)
    {
        return MessagePackSerializer.Deserialize<T>(bytes, MessagePackSourceGeneratedOptions);
    }

    private static void MessagePackSourceGeneratedSerializeToBuffer<T>(T value, ArrayBufferWriter<byte> bufferWriter)
    {
        bufferWriter.Clear();
        MessagePackSerializer.Serialize(bufferWriter, value, MessagePackSourceGeneratedOptions);
    }

    private static byte[] NinoSerialize<T>(T value)
    {
        return NinoSerializer.Serialize(value);
    }

    private static T NinoDeserialize<T>(byte[] bytes)
    {
        return NinoDeserializer.Deserialize<T>(bytes);
    }

    private static string GetBuildConfiguration()
    {
        AssemblyConfigurationAttribute? attribute = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyConfigurationAttribute>();
        return attribute?.Configuration ?? "Unknown";
    }

    private static bool GetRuntimeSwitch(string switchName)
    {
        return AppContext.TryGetSwitch(switchName, out bool enabled) && enabled;
    }
}
