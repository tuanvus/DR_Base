using DR.Common.OperationHandler;
using log4net;
using MessagePack;
using System;
using DR.Dto;

namespace DR_Sever
{
    public enum GameOpCode : ushort
    {
        DemoPing = 10001
    }

    /// <summary>
    /// Demo opcode for testing the OperationHandler flow end-to-end.
    /// Send tag 10001 / GameOpCode.DemoPing with a MessagePack payload matching DemoPingRequestDto.
    /// </summary>
    [OperationHandler(GameOpCode.DemoPing)]
    public class DemoPingHandler : OperationHandler<DemoPingRequestDto, DemoPingResponseDto>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DemoPingHandler));

        public override DemoPingResponseDto Handle(DemoPingRequestDto request)
        {
            string incomingMessage = request?.Message ?? string.Empty;

            //try
            //{
            //    LOG.Info($"DemoPingHandler received message: {incomingMessage}");
            //}
            //catch
            //{
            //    // Keep request handling alive even if logger setup is broken.
            //}

            return new DemoPingResponseDto
            {
                Success = true,
                Reply = $"  send test: ",
                ServerTicksUtc = DateTime.UtcNow.Ticks
            };
        }
    }
}
