using DarkRift;
using DR.Common.Serialization;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DR_Sever
{
    public static class MessagePackExtensions
    {
        public static void WriteMessagePack<T>(this DarkRiftWriter writer, T obj)
        {
            try
            {
                // Dùng singleton serializer
                byte[] bytes = MessagePackDtoSerializer.Instance.Serialize(obj);

                // Write length prefix
                writer.Write(bytes.Length);

                // Write data
                writer.Write(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DarkRift] WriteMessagePack error: {ex.Message}");
                throw;
            }
        }

        // Read MessagePack object từ DarkRiftReader
        public static T ReadMessagePack<T>(this DarkRiftReader reader)
        {
            try
            {
                // Read length prefix
                int length = reader.ReadInt32();

                // Read byte array
                byte[] bytes = reader.ReadBytes();

                // Deserialize với singleton serializer
                return MessagePackDtoSerializer.Instance.Deserialize<T>(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DarkRift] ReadMessagePack error: {ex.Message}");
                throw;
            }
        }

        // Read với offset/size (nếu cần optimization)
        public static T ReadMessagePackSegment<T>(this DarkRiftReader reader)
        {
            try
            {
                int length = reader.ReadInt32();
                byte[] bytes = reader.ReadBytes();

                return MessagePackDtoSerializer.Instance.Deserialize<T>(bytes, 0, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DarkRift] ReadMessagePackSegment error: {ex.Message}");
                throw;
            }
        }
    }
}
