using DR.Common.OperationHandler;

namespace Share
{
    /// <summary>
    /// Shared application base / logic.
    /// Put common OperationHandler + PacketProcessor wiring here
    /// so DR_Sever (and other servers) can reuse.
    /// </summary>
    public class Application
    {
        // Example fields (adjust visibility / initialization as needed)
        protected readonly OperationHandlerRegistry _handlerRegistry;
        protected readonly PacketProcessor _packetProcessor;

        public Application()
        {
            _handlerRegistry = new OperationHandlerRegistry();
            _packetProcessor = new PacketProcessor(_handlerRegistry);

            // TODO: Call _handlerRegistry.RegisterHandlers(...) with appropriate assemblies
        }

        // Example usage of the line you mentioned (for reference):
        // object response = _packetProcessor.ProcessPacket(handler, requestData);
    }
}
```
