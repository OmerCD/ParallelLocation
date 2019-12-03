using System.Reflection;

namespace ReflectorO
{
    internal struct PropertyStructure
    {
        public byte[] Bytes { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public int? ArraySize { get; set; }

        public PropertyStructure(byte[] bytes, PropertyInfo propertyInfo, int? arraySize)
        {
            Bytes = bytes;
            PropertyInfo = propertyInfo;
            ArraySize = arraySize;
        }
    }
}