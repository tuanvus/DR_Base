using DR.Common.OperationHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DR.Common.OperationHandler
{
    /// <summary>
    /// Registry that scans assemblies for classes marked with [OperationHandler(opcode)]
    /// and maps them to their operation code.
    /// </summary>
    public class OperationHandlerRegistry
    {
        private readonly Dictionary<ushort, Type> _handlers = new Dictionary<ushort, Type>();
        private readonly Dictionary<ushort, object> _handlerInstances = new Dictionary<ushort, object>();

        /// <summary>
        /// Register all handlers from the given assemblies.
        /// Call this at startup (and from Hotfix assemblies when loading new versions).
        /// </summary>
        public void RegisterHandlers(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            Console.WriteLine($"\n========== REGISTERING HANDLERS ==========");

            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(type => type.GetCustomAttribute<OperationHandlerAttribute>() != null)
                    .ToList();

                foreach (var handlerType in handlerTypes)
                {
                    var attribute = handlerType.GetCustomAttribute<OperationHandlerAttribute>();
                    var opCode = attribute.OperationCode;
                    var opLabel = GetOperationLabel(opCode, attribute.OperationName);

                    if (_handlers.ContainsKey(opCode))
                    {
                        Console.WriteLine($"[WARN] Duplicate opcode {opLabel} - overwriting with {handlerType.Name}");
                    }

                    _handlers[opCode] = handlerType;
                  //  Console.WriteLine($"✓ Registered: OpCode={opLabel} -> {handlerType.FullName} (from {assembly.GetName().Name})");
                }
            }

            Console.WriteLine($"Total handlers registered: {_handlers.Count}\n");
        }

        public object GetHandler(ushort operationCode)
        {
            if (!_handlers.TryGetValue(operationCode, out var handlerType))
            {
                throw new Exception($"No handler found for operation code: {operationCode}");
            }

            // Simple singleton cache (stateless handlers are fine). 
            // For stateful or per-request, replace with a factory later.
            if (!_handlerInstances.TryGetValue(operationCode, out var instance))
            {
                instance = Activator.CreateInstance(handlerType);
                _handlerInstances[operationCode] = instance;
            }

            return instance;
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
                Console.WriteLine($"OpCode {GetOperationLabel(kvp.Key, null)}: {kvp.Value.FullName}");
            }
            Console.WriteLine("=========================================\n");
        }

        public void ClearCache()
        {
            _handlerInstances.Clear();
        }

        private static string GetOperationLabel(ushort opCode, string explicitName)
        {
            if (!string.IsNullOrWhiteSpace(explicitName))
            {
                return $"{opCode} ({explicitName})";
            }

            return opCode.ToString();
        }
    }
}
