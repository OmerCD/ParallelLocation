using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ReflectorO.Attributes;

namespace ReflectorO
{
    internal class PropertySizer : IEnumerable<(PropertyInfo Property, int Size)>
    {
        private readonly Type _headerType;
        private readonly Dictionary<string, object> _valueHolder;
        private readonly IDictionary<Type, PropertyInfo[]> _typeProperties;

        public PropertySizer(Type headerType, IDictionary<Type, PropertyInfo[]> typeProperties)
        {
            _headerType = headerType;
            _typeProperties = typeProperties;
            _valueHolder = new Dictionary<string, object>();
        }

        public void AddControlObject(string propertyName, object value)
        {
            _valueHolder.Add(propertyName, value);
        }

        public object GetFromHistory(string propertyName) =>
            _valueHolder.TryGetValue(propertyName, out object @object) ? @object : null;


        private IEnumerable<(PropertyInfo Property, int Size)> GetHeadersAndSizes(Type headerType)
        {
            var properties = _typeProperties[headerType];
            for (int i = 0; i < properties.Length; i++)
            {
                var propertyType = properties[i].PropertyType;
                switch (Type.GetTypeCode(propertyType))
                {
                    case TypeCode.Byte:
                    case TypeCode.Boolean:
                        yield return (properties[i], 1);
                        break;
                    case TypeCode.DateTime:
                        yield return (properties[i], 8);
                        break;
                    case TypeCode.Char:
                        yield return (properties[i], 2);
                        break;
                    default:
                        if (propertyType.IsEnum)
                        {
                            yield return (properties[i], sizeof(int));
                        }
                        else if (propertyType.IsArray)
                        {
                            var attribute = properties[i].GetCustomAttribute<ArrayAttribute>();
                            if (attribute != null)
                            {
                                if (attribute.Lengths == null || attribute.Lengths.Length == 0)
                                {
                                    if (_valueHolder.TryGetValue(attribute.LengthPropertyName, out object length))
                                    {
                                        attribute.Lengths = new[] {(int) length};
                                    }
                                }

                                int multiplier = attribute.Lengths.Aggregate(1, (current, length) => current * length);

                                yield return (properties[i],
                                    Elector.GetSize(propertyType.GetElementType(), _typeProperties) * multiplier);
                            }
                        }
                        else if (propertyType.IsPrimitive)
                        {
                            yield return (properties[i], Marshal.SizeOf(propertyType));
                        }
                        else if (propertyType.IsValueType || propertyType.IsClass)
                        {
                            var results = GetHeadersAndSizes(propertyType);
                            var sum = results.Sum(x => x.Size);
                            yield return (properties[i], sum);
                        }
                        else
                        {
                            yield return (properties[i], Marshal.SizeOf(propertyType));
                        }

                        break;
                }
            }
        }

        public IEnumerator<(PropertyInfo Property, int Size)> GetEnumerator()
        {
            return GetHeadersAndSizes(_headerType).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}