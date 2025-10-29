using MessagePack.Resolvers;
using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DR_Sever.Serialization
{
    public class MessagePackDtoSerializer
    {
        private static MessagePackDtoSerializer instance;
        public static MessagePackDtoSerializer Instance => instance ?? (instance = new MessagePackDtoSerializer());

        private MessagePackSerializerOptions options;

        private MessagePackDtoSerializer()
        {
            // Register custom resolvers (nếu có generated code)
            StaticCompositeResolver.Instance.Register(
                // DefineMessagePackGenerated.Instance,     // Generated resolver từ mpc
                StandardResolver.Instance                   // Fallback standard resolver
            );

            options = MessagePackSerializerOptions.Standard
                .WithResolver(StaticCompositeResolver.Instance)
                .WithCompression(MessagePackCompression.Lz4BlockArray); // Optional: compression

            MessagePackSerializer.DefaultOptions = options;

            Console.WriteLine("[MessagePack] Serializer initialized");
        }

        // Serialize object thành byte array
        public byte[] Serialize<T>(T data)
        {
            try
            {
                return MessagePackSerializer.Serialize(data, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessagePack] Serialize error: {ex.Message}");
                throw;
            }
        }

        // Deserialize từ byte array
        public T Deserialize<T>(byte[] buffer)
        {
            return Deserialize<T>(buffer, 0, buffer.Length);
        }

        // Deserialize từ byte array với offset và size
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

        // Serialize trực tiếp vào IBufferWriter (zero-copy)
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
