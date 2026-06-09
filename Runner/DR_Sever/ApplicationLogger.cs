using log4net;
using System;

namespace DR_Sever
{
    public static class ApplicationLogger
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        public static void Initialize()
        {
        }

        public static void Info(string message)
        {
            Console.WriteLine($"[Application][INFO] {message}");

            try
            {
                Log.Info(message);
            }
            catch
            {
                // Do not let logging failures break packet handling.
            }
        }

        public static void Warn(string message)
        {
            Console.WriteLine($"[Application][WARN] {message}");

            try
            {
                Log.Warn(message);
            }
            catch
            {
            }
        }

        public static void Error(string message, Exception ex)
        {
            Console.WriteLine($"[Application][ERROR] {message}");
            Console.WriteLine(ex.ToString());

            try
            {
                Log.Error(message, ex);
            }
            catch
            {
            }
        }
    }
}
