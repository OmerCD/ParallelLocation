using System;
using System.Collections.Generic;
using MessageObjectRouter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parallel.Repository;
using NLog.Extensions.Logging;
using Parallel.Application.Hubs;
using Parallel.Application.Services;
using Parallel.Application.ValueObjects;
using Parallel.Location;
using Parallel.Main.Extensions;
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
            
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });
            services.AddSingleton<IDatabaseContext>(new MongoContext(appSettings.DatabaseInfo.MongoDatabase.ConnectionString, appSettings.DatabaseInfo.MongoDatabase.DatabaseName));
            services.AddSingleton<IElector, Elector>();
            services.AddElectorParser();
            services.AddLocationCalculatorRouter();
            services.AddProcessManager();
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