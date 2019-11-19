using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parallel.Repository;
using NLog.Extensions.Logging;

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
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });
            services.AddSingleton<IDatabaseContext>(new MongoContext("mongodb://188.132.230.218","TestDatabase"));
        }
    }
}