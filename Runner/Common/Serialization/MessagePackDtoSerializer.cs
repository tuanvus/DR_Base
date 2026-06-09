using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Buffers;

namespace DR.Common.Serialization
{
    public class MessagePackDtoSerializer
    {
        private static MessagePackDtoSerializer instance;
        public static MessagePackDtoSerializer Instance => instance ?? (instance = new MessagePackDtoSerializer());

        private readonly MessagePackSerializerOptions options;

        private MessagePackDtoSerializer()
        {
            StaticCompositeResolver.Instance.Register(
                StandardResolver.Instance
            );

            options = MessagePackSerializerOptions.Standard
                .WithResolver(StaticCompositeResolver.Instance)
                .WithCompression(MessagePackCompression.Lz4BlockArray);

            MessagePackSerializer.DefaultOptions = options;
        }

        public byte[] Serialize<T>(T data, bool contractless = false)
        {
            try
            {
                var resolverOptions = contractless 
                    ? MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance)
                    : options;
                return MessagePackSerializer.Serialize(data, resolverOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagePack] Serialize error: {ex.Message}");
                throw;
            }
        }

        public T Deserialize<T>(byte[] buffer)
        {
            return Deserialize<T>(buffer, 0, buffer.Length);
        }

        public object Deserialize(Type type, byte[] buffer)
        {
            return Deserialize(type, buffer, 0, buffer.Length);
        }

        public T Deserialize<T>(byte[] buffer, int offset, int size)
        {
            try
            {
                var segment = new ArraySegment<byte>(buffer, offset, size);
                return MessagePackSerializer.Deserialize<T>(segment, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagePack] Deserialize error: {ex.Message}");
                throw;
            }
        }

        public object Deserialize(Type type, byte[] buffer, int offset, int size, bool contractless = false)
        {
            try
            {
                var segment = new ArraySegment<byte>(buffer, offset, size);
                var resolverOptions = contractless 
                    ? MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance)
                    : options;
                return MessagePackSerializer.Deserialize(type, segment, resolverOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagePack] Deserialize error: {ex.Message}");
                throw;
            }
        }

        public void SerializeToWriter<T>(IBufferWriter<byte> writer, T data)
        {
            try
            {
                MessagePackSerializer.Serialize(writer, data, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagePack] SerializeToWriter error: {ex.Message}");
                throw;
            }
        }
    }
}
