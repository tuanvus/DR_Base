using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DR_Hive.OperationHandler
{
    public abstract class OperationHandler<TRequest, TResponse>
    {
        public abstract TResponse Handle(TRequest request, TResponse response);
    }


}
