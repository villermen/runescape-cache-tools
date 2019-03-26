using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public static class Formatter
    {
        public static string GetValueRepresentation(object value)
        {
            if (value is bool)
            {
                return value.ToString().ToUpper();
            }

            if (value is ValueType)
            {
                return value.ToString();
            }

            if (value is string)
            {
                return $"\"{value}\"";
            }

            if (value == null)
            {
                return "";
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                return "[" + string.Join(",", enumerable.Cast<object>().Select(Formatter.GetValueRepresentation)) + "]";
            }

            // I don't know of a better way to dynamically obtain tuple values
            var valueType = value.GetType();
            if (valueType.IsGenericType)
            {
                var valueGenericType = valueType.GetGenericTypeDefinition();
                var tupleTypes = new []
                {
                    typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>),
                    typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>)
                };

                if (tupleTypes.Contains(valueGenericType))
                {
                    var tupleProperties = valueType.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                    );

                    return "(" + string.Join(",", tupleProperties
                               .Select(tupleProperty => tupleProperty.GetValue(value))
                               .Select(Formatter.GetValueRepresentation)
                           ) + ")";
                }
            }

            throw new ArgumentException("Could not convert value to string representation");
        }
    }
}
