namespace ReflectorO.CustomParse
{
    public interface ICustomParsedObject
    {
        object Parse(byte[] data, IElector elector);
    }
}