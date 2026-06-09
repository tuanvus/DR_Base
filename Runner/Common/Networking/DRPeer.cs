using DarkRift;
using DarkRift.Server;
using DR.Common.Serialization;
using System;

namespace DR.Common.Networking
{
    public class DRPeer : IDisposable
    {
        public IClient Client { get; private set; }
        public ushort Id => Client?.ID ?? 0;

        public string UserId { get; set; }

        /// <summary>
        /// Custom data bag (like Photon's peer properties or auth token).
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Called when this peer receives a raw message.
        /// In production, you usually ignore this and let the Room/Fiber handle routing.
        /// </summary>
        public event Action<DRPeer, Message> MessageReceived;

        private bool _disposed;

        /// <summary>
        /// Called by the pool when renting a peer for a new connection.
        /// </summary>
        internal void Initialize(IClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            Client = client;
            UserId = null;
            Tag = null;
            _disposed = false;

            Client.MessageReceived += OnDarkRiftMessageReceived;
        }

        /// <summary>
        /// Called by the pool when returning the peer (cleanup).
        /// </summary>
        internal void Reset()
        {
            if (Client != null)
            {
                Client.MessageReceived -= OnDarkRiftMessageReceived;
            }

            Client = null;
            UserId = null;
            Tag = null;
        }

        private void OnDarkRiftMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (_disposed || Client == null) return;

            using (Message message = e.GetMessage())
            {
                // Clone or let the handler take ownership carefully.
                // For safety in pooled scenario, we let upper layers decide.
                MessageReceived?.Invoke(this, message);
            }
        }

        /// <summary>
        /// Send a message to this client.
        /// </summary>
        public void Send<T>(ushort tag, T data, SendMode mode = SendMode.Reliable) where T : IDarkRiftSerializable
        {
            if (Client == null || _disposed) return;

            using (Message message = Message.Create(tag, data))
            {
                Client.SendMessage(message, mode);
            }
        }

        /// <summary>
        /// Send raw message (for advanced use).
        /// </summary>
        public void SendMessage(Message message, SendMode mode)
        {
            if (Client == null || _disposed) return;
            Client.SendMessage(message, mode);
        }

        public void SendMessagePackResponse(ushort tag, object response, SendMode mode = SendMode.Reliable)
        {
            if (Client == null || _disposed) return;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            using (Message message = CreateMessagePackMessage(tag, response, writer))
            {
                Client.SendMessage(message, mode);
            }
        }

        public void SendMessagePack(ushort tag, byte[] payload, SendMode mode = SendMode.Reliable)
        {
            if (Client == null || _disposed) return;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(payload.Length);
                writer.Write(payload);
                using (Message message = Message.Create(tag, writer))
                {
                    Client.SendMessage(message, mode);
                }
            }
        }

        private Message CreateMessagePackMessage(ushort tag, object response, DarkRiftWriter writer)
        {
            byte[] payload = MessagePackDtoSerializer.Instance.Serialize(response);
            writer.Write(payload.Length);
            writer.Write(payload);
            return Message.Create(tag, writer);
        }

        /// <summary>
        /// Graceful disconnect.
        /// </summary>
        public void Disconnect()
        {
            if (Client != null && !_disposed)
            {
                Client.Disconnect();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (Client != null)
            {
                Client.MessageReceived -= OnDarkRiftMessageReceived;
                Client = null;
            }
        }
    }
}
