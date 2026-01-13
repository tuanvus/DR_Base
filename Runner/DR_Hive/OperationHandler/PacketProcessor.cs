using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DR_Hive.OperationHandler
{
    public class PacketProcessor
    {
        private readonly OperationHandlerRegistry _registry;

        public PacketProcessor(OperationHandlerRegistry registry)
        {
            _registry = registry;
        }

        public void ProcessPacket(Player player, ushort operationCode, object requestData)
        {
            Console.WriteLine($"\n>>> Processing packet: OpCode={operationCode}");

            if (!_registry.TryGetHandler(operationCode, out var handler))
            {
                Console.WriteLine($"[ERROR] Unknown operation code: {operationCode}");
                return;
            }

            // Sử dụng reflection để gọi method Handle
            var handleMethod = handler.GetType()
                .GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);

            if (handleMethod != null)
            {
                var response = handleMethod.Invoke(handler, new[] { player, requestData });

                Console.WriteLine($"[RESPONSE] {response.GetType().Name}:");
                PrintObject(response);
            }
        }

        private void PrintObject(object obj)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Console.WriteLine($"  {prop.Name}: {prop.GetValue(obj)}");
            }
        }
    }

}
