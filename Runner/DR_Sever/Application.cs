using Common;
using DarkRift.Server;
using log4net;
using log4net.Core;
using System;
using log4net.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DR_Sever
{
    public class Application : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);

        // Static log4net logger cho class này
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Application));

        public Application(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            // Initialize log4net
            LogGame.Initialize();

            // Dùng DarkRift built-in logger
            Logger.Info("=== DarkRift Server Plugin Loading ===");
            Logger.Info($"Plugin Version: {Version}");

            // Dùng log4net custom logger
            LOG.Info("log4net initialized successfully!");

            // Register events
            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisConnected;

        }

        private void OnClientDisConnected(object sender, ClientDisconnectedEventArgs e)
        {
            e.Client.MessageReceived -= OnMessageReceived;

            // DarkRift logger
            Logger.Info($"Client {e.Client.ID} disconnected");

            // log4net với màu sắc
            LogGame.ClientDisconnected(e.Client.ID, "User disconnected");
            LogGame.Debug($"Active clients: {ClientManager.Count}");
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessageReceived;

            // Get client IP

            LogGame.Debug($"Active clients: {ClientManager.Count}");

        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                using (var message = e.GetMessage())
                {
                    // Log message received

                    // Your message handling logic here
                    // ...
                }
            }
            catch (Exception ex)
            {
                // Log errors bằng cả 2
                Logger.Error($"Error processing message from Client {e.Client.ID}", ex);
            }
        }
    }
}
