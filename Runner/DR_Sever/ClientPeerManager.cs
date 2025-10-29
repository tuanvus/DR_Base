using DarkRift.Server;
using DarkRift;
using DR_Sever;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DR_Sever
{
    class ClientPeerManager
    {
        private static ClientPeerManager instance;
        public static ClientPeerManager Instance => instance ?? (instance = new ClientPeerManager());

        private ConcurrentDictionary<ushort, ClientPeer> peers;

        private ClientPeerManager()
        {
            peers = new ConcurrentDictionary<ushort, ClientPeer>();
        }

        public void AddPeer(IClient client)
        {
            var peer = new ClientPeer(client);
            peers.TryAdd(client.ID, peer);
        }

        public void RemovePeer(ushort clientId)
        {
            if (peers.TryRemove(clientId, out ClientPeer peer))
            {
                peer.Disconnect();
            }
        }

        public ClientPeer GetPeer(ushort clientId)
        {
            peers.TryGetValue(clientId, out ClientPeer peer);
            return peer;
        }

        public void BroadcastMessage<T>(ushort tag, T data, SendMode mode) where T : IDarkRiftSerializable
        {
            foreach (var peer in peers.Values)
            {
                peer.SendMessage(tag, data, mode);
            }
        }
    }
}

