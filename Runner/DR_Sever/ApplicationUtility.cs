using DR.Common.OperationHandler;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DR_Sever
{
    public static class ApplicationUtility
    {
        public static string FormatPayload(object payload, int depth = 0)
        {
            if (payload == null)
            {
                return "null";
            }

            if (depth >= 2)
            {
                return payload.ToString();
            }

            var type = payload.GetType();
            if (type == typeof(string))
            {
                return $"\"{payload}\"";
            }

            if (type.IsPrimitive || payload is decimal || payload is DateTime || payload is Guid || payload is Enum)
            {
                return Convert.ToString(payload, CultureInfo.InvariantCulture);
            }

            if (payload is IEnumerable enumerable && !(payload is string))
            {
                var items = enumerable.Cast<object>()
                    .Select(item => FormatPayload(item, depth + 1));
                return $"[{string.Join(", ", items)}]";
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead)
                .Select(property => $"{property.Name}={FormatPayload(property.GetValue(payload), depth + 1)}");

            return $"{type.Name} {{ {string.Join(", ", properties)} }}";
        }

        public static Type GetRequestType(Type handlerType)
        {
            while (handlerType != null && handlerType != typeof(object))
            {
                if (handlerType.IsGenericType &&
                    handlerType.GetGenericTypeDefinition() == typeof(OperationHandler<,>))
                {
                    return handlerType.GetGenericArguments()[0];
                }

                handlerType = handlerType.BaseType;
            }

            return null;
        }
    }
}
