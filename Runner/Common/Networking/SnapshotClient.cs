using System;
using System.Collections.Concurrent;

namespace DR.Common.Networking
{
    public class SnapshotClient
    {
        private readonly DRPeer _peer;
        private readonly ConcurrentDictionary<ushort, Action<DRPeer, Message>> _handlers = new ConcurrentDictionary<ushort, Action<DRPeer, Message>>();

        public SnapshotClient(DRPeer peer)
        {
            _peer = peer;
            _peer.MessageReceived += OnMessageReceived;
        }

        public void Subscribe(ushort tag, Action<DRPeer, Message> handler)
        {
            _handlers[tag] = handler;
        }

        public void Unsubscribe(ushort tag)
        {
            _handlers.TryRemove(tag, out _);
        }

        private void OnMessageReceived(DRPeer peer, Message message)
        {
            if (_handlers.TryGetValue(message.Tag, out var handler))
            {
                handler(peer, message);
            }
        }
    }
}
