using System;
using System.Collections.Generic;

namespace MessageObjectRouter
{
    public abstract class BaseBasicRouter<TKey> : IMessageRouter<TKey>
    {
        protected readonly Dictionary<TKey, Type> RoutedObjects;

        public BaseBasicRouter()
        {
            RoutedObjects = new Dictionary<TKey, Type>();
        }
        public IMessageRouter<TKey> AddRouter<T>(TKey identification) where T : IRouted<TKey>, new()
        {
            if (RoutedObjects.ContainsKey(identification))
            {
                RoutedObjects[identification] = typeof(T);
            }
            else
            {
                RoutedObjects.Add(identification, typeof(T));
            }

            return this;
        }

        public object Route(TKey identification)
        {
            return RoutedObjects.TryGetValue(identification, out Type type) ? Activator.CreateInstance(type) : null;
        }

        public object Route(IRouted<TKey> routeObject)
        {
            return Route(routeObject.RouteId);
        }

        public void ClearRoutes()
        {
            RoutedObjects.Clear();
        }
    }
}