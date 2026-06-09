using DR.Common.Networking;
using System;
using System.Reflection;

namespace DR.Common.OperationHandler
{
    /// <summary>
    /// Central dispatcher for operation-code based packets.
    /// Call this from inside a Room's Fiber (or ClientPeer's message handler) after deserializing the payload.
    /// </summary>
    public class PacketProcessor
    {
        private readonly OperationHandlerRegistry _registry;

        public PacketProcessor(OperationHandlerRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Process an incoming packet.
        /// </summary>
        /// <param name="peer">The DRPeer that sent the message (used for sending response if needed).</param>
        /// <param name="operationCode">The opcode that identifies which handler to use.</param>
        /// <param name="requestData">The already deserialized request object (e.g. via MessagePack).</param>
        /// <param name="responseOpCode">Optional: the opcode to use when sending the response back. If null, caller is responsible for sending.</param>
        /// <returns>The response object returned by the handler (can be null).</returns>
        public object ProcessPacket(DRPeer peer, ushort operationCode, object requestData, ushort? responseOpCode = null)
        {
            if (!_registry.TryGetHandler(operationCode, out var handler))
            {
                Console.WriteLine($"[ERROR] Unknown operation code: {operationCode}");
                return null;
            }

            // Get the Handle method - after our fix it should be Handle(TRequest)
            var handleMethod = handler.GetType()
                .GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);

            if (handleMethod == null)
            {
                Console.WriteLine($"[ERROR] Handler {handler.GetType().Name} has no public Handle method.");
                return null;
            }

            object response = null;
            try
            {
                // Correct invocation: only the request
                response = handleMethod.Invoke(handler, new[] { requestData });
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine($"[ERROR] Handler threw: {ex.InnerException?.Message}");
                throw ex.InnerException ?? ex;
            }

            return response;
        }

  
    }
}
