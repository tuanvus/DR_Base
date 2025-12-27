using DarkRift.Server;
using log4net;
using PZC.Log4Net;
using System;
using System.Collections.Concurrent;
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
        public override Version Version => new Version(1, 0, 1);

        // Static log4net logger cho class này
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Application));

      //  private readonly ConcurrentDictionary<int, int> _roleBots = [];


        public Application(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            PZC.Log.LogManager.Initialize(new Log4NetFactory());

            // Dùng DarkRift built-in logger
            LOG.Info("=== DarkRift Server Plugin Loading ===");
            LOG.Info($"Plugin Version: {Version}");


            // Register events
            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisConnected;




        }

        private void OnClientDisConnected(object sender, ClientDisconnectedEventArgs e)
        {
            e.Client.MessageReceived -= OnMessageReceived;

            // DarkRift logger
            LOG.Info($"Client {e.Client.ID} disconnected");

            // log4net với màu sắc
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessageReceived;

            // Get client IP

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
                LOG.Error($"Error processing message from Client {e.Client.ID}", ex);
            }
        }
    }
}
