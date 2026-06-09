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

        private MessagePackSerializerOptions options;
        private bool isInitialized = false;

        private MessagePackDtoSerializer()
        {
            options = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray);
            MessagePackSerializer.DefaultOptions = options;
        }

        public void Initialize(params IFormatterResolver[] customResolvers)
        {
            if (isInitialized) return;
            
            var resolvers = new System.Collections.Generic.List<IFormatterResolver>(customResolvers);
            resolvers.Add(StandardResolver.Instance);
            
            StaticCompositeResolver.Instance.Register(resolvers.ToArray());
            
            options = options.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
            
            isInitialized = true;
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
