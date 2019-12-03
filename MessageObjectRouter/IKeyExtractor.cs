namespace MessageObjectRouter
{
    public interface IKeyExtractor<out TValue, in TItem>
    {
        TValue Extract(TItem item);
        
    }
}