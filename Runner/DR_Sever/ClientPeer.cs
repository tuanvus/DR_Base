using DarkRift;
using log4net;
using DR.Common.Networking;
using System;

namespace DR_Sever
{
   
    public class ClientPeer
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ClientPeer));
        public DRPeer Peer { get; private set; }

        public ClientPeer(DRPeer peer)
        {
            Peer = peer ?? throw new ArgumentNullException(nameof(peer));

            Peer.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(DRPeer peer, Message message)
        {
            // Usually do NOT process game logic here.
            // Route the message to the player's current Room's Fiber instead.
            // Example: peer.Tag as ClientPeer -> currentRoom?.Fiber.Enqueue(() => room.HandleMessage(...));
        }

        public void SendMessage<T>(ushort tag, T data, SendMode mode = SendMode.Reliable) where T : IDarkRiftSerializable
        {
            Peer.Send(tag, data, mode);
        }

        public void Disconnect()
        {
            if (Peer != null)
            {
                Peer.MessageReceived -= OnMessageReceived;
            }
        }
    }
}
