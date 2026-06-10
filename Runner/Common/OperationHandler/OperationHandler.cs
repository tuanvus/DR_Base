using System;

namespace DR.Common.OperationHandler
{
    /// <summary>
    /// Non-generic contract for operation handlers.
    /// This allows OperationHandlerRegistry and PacketProcessor to work with any handler
    /// without knowing the specific TRequest/TResponse types at compile time.
    /// </summary>
    public interface IOperationHandler
    {
        /// <summary>
        /// Handle a request (as object) and return the response (as object, or null).
        /// The implementation will cast and call the strongly-typed Handle.
        /// </summary>
        object Handle(object request);
    }

    /// <summary>
    /// Base class for typed operation handlers.
    /// Register the concrete class with [OperationHandler(opcode)].
    /// </summary>
    public abstract class OperationHandler<TRequest, TResponse> : IOperationHandler
    {
        /// <summary>
        /// Handle the request and return a response.
        /// </summary>
        public abstract TResponse Handle(TRequest request);

        /// <summary>
        /// Explicit implementation of the non-generic contract.
        /// </summary>
        object IOperationHandler.Handle(object request)
        {
            // Handle null request gracefully (some handlers may accept it)
            TRequest typedRequest = request == null ? default : (TRequest)request;
            return Handle(typedRequest);
        }
    }
}
