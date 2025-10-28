using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace Common
{
    public static class LogGame
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LogGame));
        private static bool isConfigured = false;

        // Initialize log4net
        public static void Initialize()
        {
            if (!isConfigured)
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                isConfigured = true;
                Info("Logger initialized successfully");
            }
        }

        // Colored console output methods
        public static void Trace(string message, params object[] args)
        {
            log.DebugFormat($"[TRACE] {message}", args);
        }

        public static void Debug(string message, params object[] args)
        {
            log.DebugFormat($"[DEBUG] {message}", args);
        }

        public static void Info(string message, params object[] args)
        {
            log.InfoFormat($"[INFO] {message}", args);
        }

        public static void Warn(string message, params object[] args)
        {
            log.WarnFormat($"[WARN] {message}", args);
        }

        public static void Error(string message, Exception ex = null)
        {
            if (ex != null)
                log.Error($"[ERROR] {message}", ex);
            else
                log.Error($"[ERROR] {message}");
        }

        public static void Fatal(string message, Exception ex = null)
        {
            if (ex != null)
                log.Fatal($"[FATAL] {message}", ex);
            else
                log.Fatal($"[FATAL] {message}");
        }

        // Client connection logging với màu
        public static void ClientConnected(ushort clientId, string ipAddress)
        {
            Info($"✓ Client [{clientId}] connected from {ipAddress}");
        }

        public static void ClientDisconnected(ushort clientId, string reason = "")
        {
            Warn($"✗ Client [{clientId}] disconnected. Reason: {reason}");
        }

        // Message logging
        public static void MessageReceived(ushort clientId, ushort tag, int dataLength)
        {
            Debug($"← Message received from Client [{clientId}] | Tag: {tag} | Size: {dataLength} bytes");
        }

        public static void MessageSent(ushort clientId, ushort tag, int dataLength)
        {
            Debug($"→ Message sent to Client [{clientId}] | Tag: {tag} | Size: {dataLength} bytes");
        }
    }
}
