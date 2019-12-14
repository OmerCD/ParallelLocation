﻿using System;
using System.Collections.Generic;
using MessageObjectRouter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parallel.Repository;
using NLog.Extensions.Logging;
using Parallel.Location;
using Parallel.Main.Extensions;
using ReflectorO;
using SocketCommunication.BusinessLogic;
using SocketCommunication.Interfaces;

namespace Parallel.Main
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });
            services.AddSingleton<IDatabaseContext>(new MongoContext("mongodb://188.132.230.218", "TestDatabase"));
            services.AddSingleton<IElector, Elector>();
            services.AddScoped<IReceiver, Receiver>();
            services.AddElectorParser();
            services.AddLocationCalculatorRouter();
        }

        public void Configure(IApplicationBuilder app)
        {
           
        }
    }
}