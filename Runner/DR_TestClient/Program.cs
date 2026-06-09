using System;
using System.Threading.Tasks;
using MessagePack;

namespace DR_TestClient
{
    // ==================== MAIN ====================
    internal static class Program
    {
        public enum GameOpCode : ushort
        {
            DemoPing = 10001,
            SnapshotDemo = 20001
        }

        private static async Task Main(string[] args)
        {
            string host = args.Length > 0 ? args[0] : "127.0.0.1";
            int port = args.Length > 1 ? int.Parse(args[1]) : 4296;

            // Khởi tạo Serializer dùng chung cho toàn hệ thống với Resolver tự sinh của Dto
            Define.Serialization.MessagePackDtoSerializer.Instance.Initialize(
                DR.Dto.GeneratedMessagePackResolver.Instance
            );

            using (var network = new NetworkClient())
            {
                network.Disconnected += () => Console.WriteLine("Network disconnected.");

                var client = network.Client;

                // === MODE 2: Snapshot === (PHẢI ĐĂNG KÝ TRƯỚC KHI CONNECT ĐỂ KHÔNG BỊ MISS EVENT)
                //var snapClient = new Network.ClientSnapshotClient(client);
                //snapClient.Subscribe(20001, msg =>
                //{
                //    using (var reader = msg.GetReader())
                //    {
                //        int len = reader.ReadInt32();
                //        byte[] payload = reader.ReadBytes();
                //        // Dùng chung cấu hình serializer
                //        var snap = MessagePackSerializer.Deserialize<DR.Dto.DemoPingResponseDto>(payload.AsSpan(0, len).ToArray());
                //        Console.WriteLine($"[Snapshot] Received: {snap.Reply}");
                //    }
                //});

                // Connect sau khi đã subscribe
                network.Connect(host, port);

                // === MODE 1: API-style (chỉ 1 callback) ===
                // Bắn request, server trả lời (cùng tag) → callback được gọi ngay.
                // Không có onError riêng, vì response DTO đã có mã code + data bên trong.
                // Timeout/lỗi → callback vẫn được gọi với default (thường là null).
                var opClient = new Network.ClientOperationClient(client);
                opClient.SendAsync<DR.Dto.DemoPingRequestDto, DR.Dto.DemoPingResponseDto>(
                    tag: 10001,
                    request: new DR.Dto.DemoPingRequestDto { Message = " send to client" },
                    callback: res =>
                    {
                        Console.WriteLine($"API Result: {res?.Reply}");
                        // Bạn có thể check res == null hoặc mã code lỗi bên trong DTO ở đây
                    }
                );

                Console.WriteLine("Done. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
