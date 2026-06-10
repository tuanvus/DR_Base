using DarkRift;
using DarkRift.Server;
using DR.Common.Networking;
using DR.Common.OperationHandler;
using Define.Serialization;
using MessagePack;
using PZC.Log4Net;
using System;

namespace DR_Sever
{
    public class Application : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 1);

        private readonly OperationHandlerRegistry _handlerRegistry;
        private readonly PacketProcessor _packetProcessor;

        private System.Threading.Timer _snapshotTimer;

        public Application(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _handlerRegistry = new OperationHandlerRegistry();
            _packetProcessor = new PacketProcessor(_handlerRegistry);

            PZC.Log.LogManager.Initialize(new Log4NetFactory());
            ApplicationLogger.Initialize();

            // Khởi tạo Serializer dùng chung cho toàn hệ thống
            MessagePackDtoSerializer.Instance.Initialize();

            ApplicationLogger.Info("=== DarkRift Server Plugin Loading ===");
            ApplicationLogger.Info($"Plugin Version: {Version}");

            RegisterOperationHandlers();

            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisConnected;

            _snapshotTimer = new System.Threading.Timer(OnSnapshotTimerTick, null, 3000, 3000);
        }

        private void OnSnapshotTimerTick(object state)
        {
            foreach (var cp in ClientPeerManager.Instance.GetAllPeers())
            {
                SendSnapshotDemo(cp.Peer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _snapshotTimer?.Dispose();
            }
        }

        private void RegisterOperationHandlers()
        {
            _handlerRegistry.RegisterHandlers(GetType().Assembly);
            _handlerRegistry.PrintRegisteredHandlers();
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var clientPeer = ClientPeerManager.Instance.AddPeer(e.Client);
            clientPeer.Peer.MessageReceived += OnPeerMessageReceived;

            ApplicationLogger.Info($"Client {e.Client.ID} connected (ClientPeer wrapping pooled DRPeer). Total active: ???");

            // Demo: Gửi snapshot ngay khi client connect (tag 20001)
            SendSnapshotDemo(clientPeer.Peer);
        }

        private void SendSnapshotDemo(DRPeer peer)
        {
            if (peer == null) return;

            var snapshot = new DR.Dto.DemoPingResponseDto
            {
                Success = true,
                Reply = "Hello from server snapshot!",
                ServerTicksUtc = DateTime.UtcNow.Ticks
            };

            byte[] payload = MessagePackDtoSerializer.Instance.Serialize(snapshot);
            peer.SendMessagePack(20001, payload);
            ApplicationLogger.Info($"Sent SnapshotDemo to client {peer.Id}");
        }

        private void OnClientDisConnected(object sender, ClientDisconnectedEventArgs e)
        {
            ApplicationLogger.Info($"Client {e.Client.ID} disconnected");
            var peer = ClientPeerManager.Instance.GetDRPeer(e.Client.ID);
            if (peer != null)
            {
                peer.MessageReceived -= OnPeerMessageReceived;
            }

            ClientPeerManager.Instance.RemovePeer(e.Client.ID);
        }

        private void OnPeerMessageReceived(DRPeer peer, Message message)
        {
            ApplicationLogger.Info($"Received message from client {peer.Id}, tag={message.Tag}");
            ProcessIncomingMessage(peer, message);
        }

        private void ProcessIncomingMessage(DRPeer peer, Message message)
        {
            if (peer == null)
            {
                ApplicationLogger.Warn($"Could not resolve DRPeer for incoming tag={message.Tag}");
                return;
            }

            if (!_handlerRegistry.TryGetHandler(message.Tag, out var handler))
            {
                ApplicationLogger.Warn($"No operation handler registered for tag={message.Tag}");
                return;
            }

            var requestType = ApplicationUtility.GetRequestType(handler.GetType());
            if (requestType == null)
            {
                ApplicationLogger.Warn($"Could not infer request type for handler {handler.GetType().FullName}");
                return;
            }

            try
            {
                using (var reader = message.GetReader())
                {
                    int payloadLength = reader.ReadInt32();
                    byte[] payload = reader.ReadBytes();
                    object requestData = MessagePackDtoSerializer.Instance.Deserialize(requestType, payload, 0, payloadLength, contractless: true);
                    ApplicationLogger.Info($"Client {peer.Id} -> Server tag={message.Tag}, payload={ApplicationUtility.FormatPayload(requestData)}");

                    object response = _packetProcessor.ProcessPacket(handler, requestData);
                    if (peer != null && response != null)
                    {
                        ApplicationLogger.Info($"Server -> Client {peer.Id} tag={message.Tag}, payload={ApplicationUtility.FormatPayload(response)}");
                        peer.SendMessagePackResponse(message.Tag, response);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Error($"Failed to process client {peer.Id} message tag={message.Tag}", ex);
            }
        }
    }
}
