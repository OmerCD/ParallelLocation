namespace MessageObjectRouter
{
    public interface IRouted<out TId>
    {
        TId RouteId { get; }
    }
}