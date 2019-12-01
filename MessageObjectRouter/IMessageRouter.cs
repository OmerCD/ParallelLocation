namespace MessageObjectRouter
{
    public interface IMessageRouter<in TId>
    {
        IMessageRouter<TId> AddRouter<T>(TId identification) where T :IRouted<TId>, new();
        object Route(TId identification);
        object Route(IRouted<TId> routeObject);
        void ClearRoutes();
    }
}