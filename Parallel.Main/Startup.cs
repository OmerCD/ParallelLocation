using System;
using System.Collections.Generic;
using MessageObjectRouter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parallel.Repository;
using NLog.Extensions.Logging;
using ReflectorO;

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

            services.AddSingleton<IKeyExtractor<byte[], byte[]>>(x => new ByteKeyExtractor(new Dictionary<byte, int>
            {
                {98, 2},
                {4, 1}
            }));

            services.AddSingleton<IParseRouter<byte[]>>(x => new ByteParseRouter<byte[]>(
                x.GetRequiredService<IKeyExtractor<byte[], byte[]>>(), new Dictionary<byte[], Type>
                {
                    {new byte[] {98, 9}, typeof(int)}
                }));
            services.AddSingleton<IElector, Elector>();
        }

        public void Configure(IApplicationBuilder app)
        {
            
        }
    }
}