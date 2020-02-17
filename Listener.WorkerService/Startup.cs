using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Parallel.Application.ValueObjects;
using Parallel.Shared.Credentials;
using QueueManagement;
using QueueManagement.RabbitMQ;
using RabbitMQ.Client.Events;

namespace Listener.WorkerService
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();

            
            services.AddSingleton(appSettings);
            // services.AddSingleton<>()

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });

            // RabbitMQ injection
            services.AddSingleton<IQueueOperation<BasicDeliverEventArgs>>(
                new QueueOperation(new QueueCredential(appSettings.QueueConnectInfo.HostName,
                    appSettings.QueueConnectInfo.UserName, appSettings.QueueConnectInfo.Password)));
        }
        
    }
}