#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Parallel.Application.Services.Interfaces;
using Parallel.Application.Services.MessageProcessors;

namespace Parallel.Application.Services
{
    public class ProcessorManager : IProcessManager
    {
        private readonly HashSet<IMessageProcessor> _messageProcessors;

        public ProcessorManager(IEnumerable<IMessageProcessor>? messageProcessors = null)
        {
            _messageProcessors = messageProcessors == null
                ? new HashSet<IMessageProcessor>()
                : new HashSet<IMessageProcessor>(messageProcessors);
        }

        public void Handle(object handlingObject)
        {
            foreach (IMessageProcessor messageProcessor in _messageProcessors)
            {
                if (messageProcessor.HandleConditionSatisfied(handlingObject))
                {
                    messageProcessor.Handle(handlingObject);
                }
            }
        }

        public void RegisterProcess(IMessageProcessor messageProcessor)
        {
            if (!_messageProcessors.Contains(messageProcessor))
            {
                _messageProcessors.Add(messageProcessor);
            }
        }
    }

    public static class ProcessManagerExtensions
    {
        public static IServiceCollection AddProcessManager(this IServiceCollection services)
        {
            var interfaceType = typeof(IMessageProcessor);
            var messageServiceType = typeof(IMessageProcessor<>);
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface);
            var serviceTypes = new List<Type>();
            foreach (Type type in types)
            {
                var serviceType = messageServiceType.MakeGenericType(type);
                services.AddSingleton(serviceType, type);
                serviceTypes.Add(serviceType);
            }

            services.AddSingleton<IProcessManager, ProcessorManager>(provider =>
            {
                var provided = serviceTypes.Select(x => (IMessageProcessor)provider.GetService(x));
                var providedArray = provided.ToArray();
                return new ProcessorManager(
                    providedArray
                );
            });
            return services;
        }
    }
}