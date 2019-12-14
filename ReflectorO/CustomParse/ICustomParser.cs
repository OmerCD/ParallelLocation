using System;
using System.Runtime.InteropServices;

namespace ReflectorO.CustomParse
{
    public interface ICustomParser
    {
        object Parse(ParseInfo parseInfo);
    }

    public struct ParseInfo
    {
        public ParseInfo(byte[] data, object parsedObject, int currentIndex, IElector electorParser)
        {
            Data = data;
            ParsedObject = parsedObject;
            CurrentIndex = currentIndex;
            ElectorParser = electorParser;
            ContinueIndex = -1;
        }

        public byte[] Data { get;}
        public object ParsedObject { get;}
        public int CurrentIndex { get;}
        public IElector ElectorParser { get; set; }
        public int ContinueIndex { get; set; }
    }
[AttributeUsage(AttributeTargets.Property)]
    public class CustomParserAttribute : Attribute
    {
        public CustomParserAttribute(Type objectParserType, bool receiveAllData = false)
        {
            ObjectParserType = objectParserType;
            ReceiveAllData = receiveAllData;
        }
        public bool ReceiveAllData { get;}
        public Type ObjectParserType { get; }
    }
}