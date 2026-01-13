using DR_Hive.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DR_Hive.OperationHandler
{
    public class OperationHandlerRegistry
    {
        private readonly Dictionary<ushort, Type> _handlers = new Dictionary<ushort, Type>();
        private readonly Dictionary<ushort, object> _handlerInstances = new Dictionary<ushort, object>();

        public void RegisterHandlers(Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetExecutingAssembly();

            // Scan tất cả các class có OperationHandlerAttribute
            var handlerTypes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<OperationHandlerAttribute>() != null)
                .ToList();

            Console.WriteLine($"\n========== REGISTERING HANDLERS ==========");

            foreach (var handlerType in handlerTypes)
            {
                var attribute = handlerType.GetCustomAttribute<OperationHandlerAttribute>();
                var opCode = attribute.OperationCode;

                _handlers[opCode] = handlerType;

                Console.WriteLine($"✓ Registered: OpCode={opCode} -> {handlerType.Name}");
            }

            Console.WriteLine($"Total handlers registered: {_handlers.Count}\n");
        }

        public object GetHandler(ushort operationCode)
        {
            if (!_handlers.ContainsKey(operationCode))
            {
                throw new Exception($"No handler found for operation code: {operationCode}");
            }

            // Sử dụng singleton pattern (có thể thay bằng pool)
            if (!_handlerInstances.ContainsKey(operationCode))
            {
                _handlerInstances[operationCode] = Activator.CreateInstance(_handlers[operationCode]);
            }

            return _handlerInstances[operationCode];
        }

        public bool TryGetHandler(ushort operationCode, out object handler)
        {
            handler = null;

            if (!_handlers.ContainsKey(operationCode))
                return false;

            handler = GetHandler(operationCode);
            return true;
        }

        public void PrintRegisteredHandlers()
        {
            Console.WriteLine("\n========== REGISTERED HANDLERS ==========");
            foreach (var kvp in _handlers.OrderBy(x => x.Key))
            {
                Console.WriteLine($"OpCode {kvp.Key}: {kvp.Value.Name}");
            }
            Console.WriteLine("=========================================\n");
        }
    }
}
