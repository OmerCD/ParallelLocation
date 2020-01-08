using System.Reflection;
using ReflectorO.CustomParse;

namespace ReflectorO
{
    internal struct PropPair
    {
        public PropPair(PropertyInfo propertyInfo, ICustomParser customParser)
        {
            PropertyInfo = propertyInfo;
            CustomParser = customParser;
        }

        public PropertyInfo PropertyInfo { get; set; }
        public ICustomParser CustomParser { get; set; }
    }
}