using DarkRift.Client;
using System;
using System.Collections.Concurrent;
using DarkRift;

namespace DR_TestClient.Network
{
    public class ClientSnapshotClient
    {
        private readonly DarkRiftClient _client;
        private readonly ConcurrentDictionary<ushort, Action<Message>> _handlers = new ConcurrentDictionary<ushort, Action<Message>>();

        public ClientSnapshotClient(DarkRiftClient client)
        {
            _client = client;
            _client.MessageReceived += OnMessageReceived;
        }

        public void Subscribe(ushort tag, Action<Message> handler) => _handlers[tag] = handler;
        public void Unsubscribe(ushort tag) => _handlers.TryRemove(tag, out _);

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var msg = e.GetMessage())
            {
                if (_handlers.TryGetValue(msg.Tag, out var handler))
                    handler(msg);
            }
        }
    }
}
