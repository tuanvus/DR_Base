using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DR_Hive.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OperationHandlerAttribute : Attribute
    {
        public ushort OperationCode { get; }

        public OperationHandlerAttribute(ushort operationCode)
        {
            OperationCode = operationCode;
        }
    }
}
