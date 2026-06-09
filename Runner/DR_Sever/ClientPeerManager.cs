using DarkRift.Server;
using DarkRift;
using System;
using System.Collections.Concurrent;

namespace DR_Sever
{
    class ClientPeerManager
    {
        private static ClientPeerManager instance;
        public static ClientPeerManager Instance => instance ?? (instance = new ClientPeerManager());

        private readonly ConcurrentDictionary<ushort, ClientPeer> _peers 
            = new ConcurrentDictionary<ushort, ClientPeer>();

        private ClientPeerManager() { }

        public ClientPeer AddPeer(IClient client)
        {
            var drPeer = DR.Common.Networking.DRPeerPool.Instance.Rent(client);

            var clientPeer = new ClientPeer(drPeer);

            _peers.TryAdd(client.ID, clientPeer);
            return clientPeer;
        }

        public void RemovePeer(ushort clientId)
        {
            if (_peers.TryRemove(clientId, out var clientPeer))
            {
                var drPeer = clientPeer.Peer;

                // TODO: Notify the player's current room via its Fiber
                // e.g. clientPeer.Peer.Tag as ... or keep room ref on ClientPeer
                // drPeer.CurrentRoom?.Enqueue(() => room.OnPlayerLeft(clientPeer));

                clientPeer.Disconnect();

                // Return the underlying DRPeer to the pool
                DR.Common.Networking.DRPeerPool.Instance.Return(drPeer);
            }
        }

        public ClientPeer GetPeer(ushort clientId)
        {
            _peers.TryGetValue(clientId, out var clientPeer);
            return clientPeer;
        }
        public DR.Common.Networking.DRPeer GetDRPeer(ushort clientId)
        {
            var cp = GetPeer(clientId);
            return cp?.Peer;
        }

        public void Broadcast<T>(ushort tag, T data, SendMode mode = SendMode.Reliable) where T : IDarkRiftSerializable
        {
            foreach (var clientPeer in _peers.Values)
            {
                clientPeer.SendMessage(tag, data, mode);
            }
        }

        public System.Collections.Generic.IEnumerable<ClientPeer> GetAllPeers()
        {
            return _peers.Values;
        }
    }
}

