using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parallel.Repository;
using NLog.Extensions.Logging;
using Parallel.Application.Hubs;
using Parallel.Application.Services;
using Parallel.Application.Services.CumulocityIOT;
using Parallel.Application.ValueObjects;
using Parallel.Main.Extensions;
using Parallel.Shared.Credentials;
using QueueManagement;
using QueueManagement.RabbitMQ;
using RabbitMQ.Client.Events;
using ReflectorO;

namespace Parallel.Main
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
            services.AddControllers();
            services.AddSingleton(appSettings);
            services.AddSingleton(appSettings.CumulocityInfo);
            
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });
            
            // RabbitMQ injection
            services.AddSingleton<IQueueOperation<BasicDeliverEventArgs>>(
                new QueueOperation(new QueueCredential(appSettings.QueueConnectInfo.HostName,
                    appSettings.QueueConnectInfo.UserName, appSettings.QueueConnectInfo.Password, appSettings.QueueConnectInfo.AMQPPort, appSettings.QueueConnectInfo.APIPort)));
            
            services.AddSingleton<IDatabaseContext>(new MongoContext(appSettings.DatabaseInfo.MongoDatabase.ConnectionString, appSettings.DatabaseInfo.MongoDatabase.DatabaseName));
            services.AddSingleton<IElector, Elector>();
            services.AddElectorParser();
            services.AddLocationCalculatorRouter();
            services.AddProcessManager();
            services.AddSingleton<DynamicQueueListener>();
            services.AddSingleton<CumulocityIOTService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseRouting();
            app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            app.UseEndpoints(endpoints => { endpoints.MapHub<LocationHub>("/location"); });
        }
    }
}