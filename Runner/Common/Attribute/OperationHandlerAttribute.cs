using System;

namespace DR.Common.OperationHandler
{
    /// <summary>
    /// Marks a class as an operation handler for a specific opcode.
    /// Place this on classes that inherit from OperationHandler&lt;TRequest, TResponse&gt;.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class OperationHandlerAttribute : System.Attribute
    {
        public ushort OperationCode { get; }
        public string OperationName { get; }

        public OperationHandlerAttribute(ushort operationCode)
        {
            OperationCode = operationCode;
        }

        public OperationHandlerAttribute(object operationCode)
        {
            if (operationCode == null)
            {
                throw new ArgumentNullException(nameof(operationCode));
            }

            if (!(operationCode is Enum operationEnum))
            {
                throw new ArgumentException("Operation code must be an enum value.", nameof(operationCode));
            }

            OperationCode = Convert.ToUInt16(operationEnum);
            OperationName = operationEnum.ToString();
        }
    }
}
