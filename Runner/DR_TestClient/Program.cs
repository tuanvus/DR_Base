using System;
using System.Threading.Tasks;
using DR_Sever;
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

            using (var network = new NetworkClient())
            {
                network.Disconnected += () => Console.WriteLine("Network disconnected.");
                network.Connect(host, port);

                var client = network.Client;

                // === MODE 1: API-style ===
                var opClient = new Network.ClientOperationClient(client);
                var res = await opClient.SendAsync<DemoPingRequest, DemoPingResponse>(10001, new DemoPingRequest { Message = "test" });
                Console.WriteLine($"API Result: {res?.Reply}");

                // === MODE 2: Snapshot ===
                var snapClient = new Network.ClientSnapshotClient(client);
                snapClient.Subscribe(20001, msg =>
                {
                    using (var reader = msg.GetReader())
                    {
                        int len = reader.ReadInt32();
                        byte[] payload = reader.ReadBytes();
                        var snap = MessagePackSerializer.Deserialize<DemoPingResponse>(payload.AsSpan(0, len).ToArray());
                        Console.WriteLine($"[Snapshot] Received: {snap.Reply}");
                    }
                });

                Console.WriteLine("Done. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
