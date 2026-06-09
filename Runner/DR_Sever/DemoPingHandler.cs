using DR.Common.OperationHandler;
using log4net;
using MessagePack;
using System;

namespace DR_Sever
{
    [MessagePackObject]
    public class DemoPingRequest
    {
        [Key(0)]
        public string Message { get; set; }
    }

    [MessagePackObject]
    public class DemoPingResponse
    {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string Reply { get; set; }

        [Key(2)]
        public long ServerTicksUtc { get; set; }
    }

    public enum GameOpCode : ushort
    {
        DemoPing = 10001
    }

    /// <summary>
    /// Demo opcode for testing the OperationHandler flow end-to-end.
    /// Send tag 10001 / GameOpCode.DemoPing with a MessagePack payload matching DemoPingRequest.
    /// </summary>
    [OperationHandler(GameOpCode.DemoPing)]
    public class DemoPingHandler : OperationHandler<DemoPingRequest, DemoPingResponse>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DemoPingHandler));

        public override DemoPingResponse Handle(DemoPingRequest request)
        {
            string incomingMessage = request?.Message ?? string.Empty;

            try
            {
                LOG.Info($"DemoPingHandler received message: {incomingMessage}");
            }
            catch
            {
                // Keep request handling alive even if logger setup is broken.
            }

            return new DemoPingResponse
            {
                Success = true,
                Reply = $"PONG: {incomingMessage}",
                ServerTicksUtc = DateTime.UtcNow.Ticks
            };
        }
    }
}
