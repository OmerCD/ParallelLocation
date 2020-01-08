using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ReflectorO.Attributes;
using ReflectorO.CustomParse;
using ReflectorO.Exceptions;

namespace ReflectorO
{
    public class Elector : IElector
    {
        private readonly IDictionary<Type, PropertyInfo[]> _typeList;
        private static readonly Type CustomParserType = typeof(ICustomParsedObject);
        private EndianType _defaultEndianType;
        private EndianBitConverter _bitConverter;

        public EndianType DefaultEndianType
        {
            get => _defaultEndianType;
            set
            {
                _defaultEndianType = value;
                _bitConverter = value switch
                {
                    EndianType.LittleEndian => EndianBitConverter.LittleEndianBitConverter,
                    EndianType.BigEndian => EndianBitConverter.BigEndianBitConverter,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }
        }

        public Elector(EndianType defaultEndianType = EndianType.LittleEndian)
        {
            DefaultEndianType = defaultEndianType;
            _typeList = new Dictionary<Type, PropertyInfo[]>();
        }

        public void RegisterType(Type type)
        {
            if (_typeList.ContainsKey(type))
            {
                throw new ElectorDuplicateTypeException(type);
            }

            AddType(type);
        }

        private void AddType(Type type)
        {
            _typeList.Add(type,
                type.GetProperties().Where(info => info.GetCustomAttribute<NotParsedAttribute>() == null)
                    .OrderBy(x => x.MetadataToken).ToArray());
        }

        public byte[] CreateByteArray(object @object)
        {
            switch (@object)
            {
                case int iObj:
                    return _bitConverter.GetBytes(iObj);
                case long lObj:
                    return _bitConverter.GetBytes(lObj);
                case bool bObj:
                    return _bitConverter.GetBytes(bObj);
                case char cObj:
                    return _bitConverter.GetBytes(cObj);
                case double dObj:
                    return _bitConverter.GetBytes(dObj);
                case float fObj:
                    return _bitConverter.GetBytes(fObj);
                case short sObj:
                    return _bitConverter.GetBytes(sObj);
                case uint uiObj:
                    return _bitConverter.GetBytes(uiObj);
                case byte bObj:
                    return new[] {bObj};
                case ushort usObj:
                    return _bitConverter.GetBytes(usObj);
                case DateTime dtObj:
                    return _bitConverter.GetBytes(dtObj.Ticks);
                default:
                    Type objType = @object.GetType();
                    if (@object is Array array)
                    {
                        var byteArray = new List<byte>();
                        foreach (object arrItem in array)
                        {
                            byte[] bytes = CreateByteArray(arrItem);
                            byteArray.AddRange(bytes);
                        }

                        return byteArray.ToArray();
                    }
                    else if (objType.IsEnum)
                    {
                        return _bitConverter.GetBytes(Convert.ToInt32(@object));
                    }
                    else if (objType.IsClass || objType.IsValueType)
                    {
                        if (!_typeList.TryGetValue(objType, out PropertyInfo[] properties))
                        {
                            AddType(objType);
                            properties = _typeList[objType];
                        }

                        var byteList = new List<byte>();
                        foreach (PropertyInfo property in properties)
                        {
                            object value = property.GetValue(@object);
                            byte[] rest = CreateByteArray(value);
                            byteList.AddRange(rest);
                        }

                        return byteList.ToArray();
                    }

                    throw new ElectorConvertException(@object.GetType().FullName, @object);
            }
        }

        public object CreateObject(byte[] bytes, Type type)
        {
            var propsAndSizes = new PropertySizer(type, _typeList);
            object header = Activator.CreateInstance(type);

            if (CustomParserType.IsAssignableFrom(type))
            {
                return ((ICustomParsedObject) header).Parse(bytes, this);
            }

            SetPropertyValues(bytes, header, propsAndSizes, 0);

            return header;
        }

        private static object CreateValue<T>(Func<byte[], int, T> func, byte[] array, bool reverse)
        {
            if (reverse)
            {
                Array.Reverse(array);
            }

            return func(array, 0);
        }

        private object ByteArrayToObject(PropertyStructure propStruct)
        {
            byte[] array = propStruct.Bytes;
            PropertyInfo propInfo = propStruct.PropertyInfo;
            Type objectType = propInfo.PropertyType;
            bool reverse = _defaultEndianType == EndianType.BigEndian;
            switch (Type.GetTypeCode(objectType))
            {
                case TypeCode.Int32:
                    return CreateValue(BitConverter.ToInt32, array, reverse);
                case TypeCode.Int64:
                    return CreateValue(BitConverter.ToInt64, array, reverse);
                case TypeCode.Boolean:
                    return CreateValue(BitConverter.ToBoolean, array, reverse);
                case TypeCode.Char:
                    return CreateValue(BitConverter.ToChar, array, reverse);
                case TypeCode.Double:
                    return CreateValue(BitConverter.ToDouble, array, reverse);
                case TypeCode.Single:
                    return CreateValue(BitConverter.ToSingle, array, reverse);
                case TypeCode.Int16:
                    return CreateValue(BitConverter.ToInt16, array, reverse);
                case TypeCode.UInt32:
                    return CreateValue(BitConverter.ToUInt32, array, reverse);
                case TypeCode.Byte:
                    return array[0];
                case TypeCode.UInt16:
                    return CreateValue(BitConverter.ToUInt16, array, reverse);
                case TypeCode.DateTime:
                    var ticks = Convert.ToInt64(CreateValue(BitConverter.ToUInt64, array, reverse));
                    return new DateTime(ticks);
                default:
                    if (objectType.IsArray)
                    {
                        var attribute = propInfo.GetCustomAttribute<ArrayAttribute>();
                        if (attribute != null)
                        {
                            if (attribute.Lengths == null || attribute.Lengths.Length == 0)
                            {
                                if (propStruct.ArraySize != null)
                                {
                                    attribute.Lengths = new[] {propStruct.ArraySize.Value};
                                }
                            }

                            Array arrayObject = CreateArray(array, objectType.GetElementType(), reverse, attribute);
                            return arrayObject;
                        }
                    }
                    else if (objectType.IsValueType)
                    {
                        return CreateObject(array, objectType);
                    }
                    else if (objectType.IsClass)
                    {
                        return CreateObject(array, objectType);
                    }

                    var printValue = new System.Text.StringBuilder();
                    for (var i = 0; i < array.Length; i++)
                    {
                        printValue.Append(array[i]);
                    }

                    throw new ElectorConvertException(objectType.FullName, printValue);
            }
        }

        public static int GetSize(Type propertyType, IDictionary<Type, PropertyInfo[]> typeProperties)
        {
            switch (Type.GetTypeCode(propertyType))
            {
                case TypeCode.Byte:
                case TypeCode.Boolean:
                    return 1;
                case TypeCode.Char:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    return 2;
                case TypeCode.UInt32:
                case TypeCode.Int32:
                    return 4;
                case TypeCode.DateTime:
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    return 8;

                default:
                    if (propertyType.IsEnum)
                    {
                        return sizeof(int);
                    }
                    else if (propertyType.IsValueType || propertyType.IsClass)
                    {
                        PropertyInfo[] properties = typeProperties[propertyType];
                        var sum = 0;
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        for (var i = 0; i < properties.Length; i++)
                        {
                            sum += GetSize(properties[i].PropertyType, typeProperties);
                        }

                        return sum;
                    }
                    else
                    {
                        return Marshal.SizeOf(propertyType);
                    }
            }
        }

        private Array CreateArray(byte[] array, Type elementType, bool reverse, ArrayAttribute attribute)
        {
            int size = GetSize(elementType, _typeList);
            var arrayObject = Array.CreateInstance(elementType, attribute.Lengths);
            long lastIndex = 0;
            var tempArray = new byte[size];

            if (arrayObject.Rank > 1)
            {
                arrayObject.ForEach((x, indices) =>
                {
                    Array.Copy(array, lastIndex, tempArray, 0, size);
                    if (reverse)
                    {
                        Array.Reverse(tempArray);
                    }

                    object @object = CreateObject(tempArray, elementType);
                    arrayObject.SetValue(@object, indices);
                    lastIndex += size;
                });
            }
            else
            {
                for (var i = 0; i < arrayObject.LongLength; i++)
                {
                    Array.Copy(array, lastIndex, tempArray, 0, size);
                    if (reverse)
                    {
                        Array.Reverse(tempArray);
                    }

                    object @object = CreateObject(tempArray, elementType);
                    arrayObject.SetValue(@object, i);
                    lastIndex += size;
                }
            }

            return arrayObject;
        }

        private long SetPropertyValues(byte[] package, object header, PropertySizer propsAndSizes,
            long lastIndex)
        {
            foreach ((PropertyInfo property, int size) in propsAndSizes)
            {
                var propValueBytes = new byte[size];
                Array.Copy(package, lastIndex, propValueBytes, 0, propValueBytes.Length);
                lastIndex += size;
                var arrayAttribute = property.GetCustomAttribute<ArrayAttribute>();
                var arraySize = 0;
                if (arrayAttribute != null && (arrayAttribute.Lengths == null || arrayAttribute.Lengths.Length == 0))
                {
                    arraySize = (int) propsAndSizes.GetFromHistory(arrayAttribute.LengthPropertyName);
                }

                var parseAttribute = property.GetCustomAttribute<CustomParserAttribute>();
                object createdObject;
                if (parseAttribute == null)
                {
                    createdObject = ByteArrayToObject(new PropertyStructure(propValueBytes, property, arraySize));
                }
                else
                {
                    var objectParser = (ICustomParser) Activator.CreateInstance(parseAttribute.ObjectParserType);
                    var parseInfo = new ParseInfo(parseAttribute.ReceiveAllData ? package : propValueBytes, header,
                        (int) lastIndex, this);
                    createdObject = objectParser.Parse(parseInfo);
                    if (parseInfo.ContinueIndex != -1)
                    {
                        lastIndex = parseInfo.ContinueIndex;
                    }
                }

                propsAndSizes.AddControlObject(property.Name, createdObject);
                property.SetValue(header, createdObject);
            }

            return lastIndex;
        }
    }
}