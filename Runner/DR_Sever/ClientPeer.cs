using DarkRift;
using DarkRift.Server;
using log4net;
using PZC.Concurrency;
using System;

namespace DR_Sever
{
    public class ClientPeer
    {

        private static readonly ILog LOG = LogManager.GetLogger(typeof(ClientPeer));
        private readonly ThreadPoolFiber _fiber;
        public IClient Client { get; private set; }
        public int INTERVAL_UPDATE = 200;

        public ClientPeer(IClient client)
        {
            Client = client;
            _fiber = new ThreadPoolFiber();
            _fiber.Start();
            _fiber.ScheduleOnInterval(Update, 1000, INTERVAL_UPDATE);
            Client.MessageReceived += OnMessageReceived;
        }

        private void Update()
        {

        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {

            }
        }
        public void SendMessage<T>(ushort tag, T data, SendMode mode) where T : IDarkRiftSerializable
        {
            using (Message message = Message.Create(tag, data))
            {
                Client.SendMessage(message, mode);
            }
        }

        public void Disconnect()
        {
            Client.MessageReceived -= OnMessageReceived;
        }
    }
}
