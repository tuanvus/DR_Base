using System;

namespace DR.Common.OperationHandler
{
    /// <summary>
    /// Base class for typed operation handlers.
    /// Register the concrete class with [OperationHandler(opcode)].
    /// </summary>
    public abstract class OperationHandler<TRequest, TResponse>
    {
        /// <summary>
        /// Handle the request and return a response.
        /// </summary>
        public abstract TResponse Handle(TRequest request);
    }
}
