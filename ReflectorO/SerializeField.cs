using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ReflectorO.CustomParse;

namespace ReflectorO
{
    internal class SerializeField : IEnumerable<PropPair>
    {
        private readonly PropPair[] _propPairs;
        private Dictionary<int, ICustomParser> _parsers;
        public SerializeField(IReadOnlyList<PropertyInfo> propertyInfos)
        {
            _propPairs = new PropPair[propertyInfos.Count];
            PropertyInfo prop;
            CustomParserAttribute attribute;
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                prop = propertyInfos[i];
                attribute = prop.GetCustomAttribute<CustomParserAttribute>();
                if (attribute != null)
                {
                    _propPairs[i].CustomParser = (ICustomParser)Activator.CreateInstance(attribute.ObjectParserType);
                }
                _propPairs[i].PropertyInfo = propertyInfos[i];
            }
            _parsers = new Dictionary<int, ICustomParser>();
        }

        public PropPair this[int index]
        {
            get => _propPairs[index];
            set => _propPairs[index] = value;
        }

        public IEnumerator<PropPair> GetEnumerator()
        {
            for (int i = 0; i < _propPairs.Length; i++)
            {
                yield return _propPairs[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}