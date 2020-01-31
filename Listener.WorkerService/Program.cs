using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Listener.WorkerService
{
    public class Program
    {
        private static IServiceCollection _serviceCollection;

        public static void Main(string[] args)
        {
            _serviceCollection = new ServiceCollection();

            static IConfigurationBuilder BuilderAction(IConfigurationBuilder builder)
            {
                return builder.SetBasePath(Path.Combine(AppContext.BaseDirectory))
#if DEBUG
                    .AddJsonFile("appsettings.Development.json", true, true);

#else
                    .AddJsonFile($"appsettings.json", true, true);
#endif
            }

             CreateHostBuilder(args, (b) => BuilderAction(b)).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args,
            Action<IConfigurationBuilder> builderAction)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builderAction)
                .ConfigureServices((hostContext, services)  =>
                {
                    services.AddHostedService<MainCycle>();
                    var configBuilder = new ConfigurationBuilder();
                    builderAction(configBuilder);
                    var cRoot = configBuilder.Build();
                    var startUp = new Startup(cRoot);
                    startUp.ConfigureServices(services);
                });
        }
    }
}