namespace MessageObjectRouter
{
    public interface IParseRouter<in T>
    {
        object GetObject(T bytes);
    }
}