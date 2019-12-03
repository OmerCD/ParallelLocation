using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Parallel.Location
{
    public class LocationRouterBuilder
    {
        private readonly IDictionary<Type, ILocationCalculator> _locationCalculators;

        public LocationRouterBuilder(IDictionary<Type, ILocationCalculator> locationCalculators)
        {
            _locationCalculators = locationCalculators;
        }

        public LocationRouterBuilder AddLocationCalculator(Type type, ILocationCalculator calculator)
        {
            if (!_locationCalculators.ContainsKey(type))
            {
                _locationCalculators.Add(type, calculator);
            }
            else
            {
                throw new Exception($"Key already registered : {type.Name}");
            }

            return this;
        }
        public LocationRouterBuilder AddLocationCalculator<T>(ILocationCalculator calculator)
        {
            var type = typeof(T);
            return AddLocationCalculator(type, calculator);
        }
    }

    public static class LocationCalculatorRouterExtensions
    {
        public static IServiceCollection AddLocationCalculatorRouter(this IServiceCollection services,
            Action<LocationRouterBuilder> locationRouterBuilder)
        {
            var locationCalculatorRouter = new LocationCalculatorRouter();
            var builder = new LocationRouterBuilder(locationCalculatorRouter.LocationCalculators);
            locationRouterBuilder(builder);
            services.AddSingleton<ILocationCalculatorRouter<Type>>(
                locationCalculatorRouter);
            return services;
        }
    }

    public class LocationCalculatorRouter : ILocationCalculatorRouter<Type>
    {
        internal IDictionary<Type, ILocationCalculator> LocationCalculators { get; }

        public LocationCalculatorRouter()
        {
            LocationCalculators = new Dictionary<Type, ILocationCalculator>();
        }

        public LocationCalculatorRouter(IDictionary<Type, ILocationCalculator> locationCalculators)
        {
            LocationCalculators = locationCalculators;
        }
        

        public ILocationCalculator GetCalculator(Type key)
        {
            if (LocationCalculators.TryGetValue(key, out ILocationCalculator calculator))
            {
                return calculator;
            }

            throw new KeyNotFoundException($"Couldn't find key : {key.Name}");
        }
    }
}