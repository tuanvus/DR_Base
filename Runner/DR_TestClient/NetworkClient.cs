using DarkRift;
using DarkRift.Client;
using System;
using System.Net;

namespace DR_TestClient
{
    /// <summary>
    /// Quản lý kết nối DarkRift Client (connect, disconnect, events)
    /// </summary>
    public class NetworkClient : IDisposable
    {
        private readonly DarkRiftClient _client;

        public DarkRiftClient Client => _client;
        public ushort ClientId => _client.ID;
        public bool IsConnected => _client.Connected;

        public event Action Disconnected;

        public NetworkClient()
        {
            _client = new DarkRiftClient();
            _client.Disconnected += OnDisconnected;
        }

        public void Connect(string host = "127.0.0.1", int port = 4296)
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            _client.Connect(IPAddress.Parse(host), port, true);
            Console.WriteLine($"Connected. ClientId={_client.ID}");
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine($"Disconnected. Local={e.LocalDisconnect}, Error={e.Error}");
            Disconnected?.Invoke();
        }

        public void Dispose()
        {
            _client?.Disconnect();
            _client?.Dispose();
        }
    }
}
