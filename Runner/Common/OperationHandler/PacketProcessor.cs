using System;

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
        /// Process a request using an already resolved <see cref="IOperationHandler"/>.
        /// Preferred overload when you have already obtained the handler (e.g. to inspect its generic
        /// arguments for deserialization of the request DTO).
        /// </summary>
        public object ProcessPacket(IOperationHandler handler, object requestData)
        {
            if (handler == null)
            {
                Console.WriteLine("[ERROR] Handler is null.");
                return null;
            }

            try
            {
                // Direct call through the interface — no reflection on the hot path.
                return handler.Handle(requestData);
            }
            catch (Exception ex)
            {
                if (ex is System.Reflection.TargetInvocationException tie)
                {
                    Console.WriteLine($"[ERROR] Handler threw: {tie.InnerException?.Message}");
                    throw tie.InnerException ?? tie;
                }

                Console.WriteLine($"[ERROR] Handler threw: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Process an incoming packet by opcode (convenience overload).
        /// Performs an internal lookup via the registry, then delegates to the <see cref="IOperationHandler"/> overload.
        /// Use the <see cref="ProcessPacket(IOperationHandler, object)"/> overload when you already have the handler instance.
        /// </summary>
        public object ProcessPacket(ushort operationCode, object requestData)
        {
            if (!_registry.TryGetHandler(operationCode, out var handler))
            {
                Console.WriteLine($"[ERROR] Unknown operation code: {operationCode}");
                return null;
            }

            return ProcessPacket(handler, requestData);
        }
    }
}
