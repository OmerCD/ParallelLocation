using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
    public class LocationRouterPropertyBasedBuilder
    {
        public class ClassFunction
        {
            public object FuncHolder { get; private set; }

            public Func<TEntity, TOutput> SetFunc<TEntity, TOutput>(Func<TEntity, TOutput> func)
            {
                FuncHolder = func;
                return func;
            }
            
        }
        private readonly IDictionary<object, ILocationCalculator> _locationCalculators;
        private readonly IDictionary<Type, ClassFunction> _typeFinders;

        public LocationRouterPropertyBasedBuilder(IDictionary<Type, ClassFunction> typeFinders, IDictionary<object, ILocationCalculator> locationCalculators)
        {
            _typeFinders = typeFinders;
            _locationCalculators = locationCalculators;
        }

        public LocationRouterPropertyBasedBuilder AddLocationCalculator<TEntity,TOutput>( Expression<Func<TEntity,TOutput>> func,object keyValue,ILocationCalculator calculator)
        {
            if (!_locationCalculators.ContainsKey(keyValue))
            {
                var cF = new ClassFunction();
                cF.SetFunc(func.Compile());
                _typeFinders.Add(typeof(TEntity), cF);
                _locationCalculators.Add(keyValue, calculator);
            }
            else
            {
                throw new Exception($"Key already registered : {keyValue}");
            }

            return this;
        }
        public static Func<TTarget, TConst> Convert<TSource, TTarget, TConst>(Expression<Func<TSource, TConst>> root)
        {
            var visitor = new ParameterTypeVisitor<TSource, TTarget>();
            var expression = (Expression<Func<TTarget, TConst>>)visitor.Visit(root);
            return expression?.Compile();
        }

        private class ParameterTypeVisitor<TSource, TTarget> : ExpressionVisitor
        {
            private ReadOnlyCollection<ParameterExpression> _parameters;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameters?.FirstOrDefault(p => p.Name == node.Name) ?? 
                       (node.Type == typeof(TSource) ? Expression.Parameter(typeof(TTarget), node.Name) : node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                _parameters = VisitAndConvert(node.Parameters, "VisitLambda");
                return Expression.Lambda(Visit(node.Body) ?? throw new InvalidOperationException(), _parameters);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == typeof(TSource))
                {
                    return Expression.Property(Visit(node.Expression) ?? throw new InvalidOperationException(), node.Member.Name);
                }
                return base.VisitMember(node);
            }
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