using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using Parallel.Location;
using Parallel.Repository;

namespace Parallel.Main
{
    class Program
    {
        private static IServiceCollection _serviceCollection;

        static void Main(string[] args)
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

           // CreateWebHostBuilder(args, (b) => BuilderAction(b)).Build().RunAsync();
            CreateHostBuilder(args, (b) => BuilderAction(b)).Build().RunAsync();
            // Console.ReadKey();
        }

        private static IHostBuilder CreateHostBuilder(string[] args,
            Action<IConfigurationBuilder> builderAction)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builderAction)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MainCycle>();
                    var configBuilder = new ConfigurationBuilder();
                    builderAction(configBuilder);
                    var cRoot = configBuilder.Build();
                    var startUp = new Startup(cRoot);
                    startUp.ConfigureServices(services);
                });
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args,
            Action<IConfigurationBuilder> builderAction)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builderAction)
                .ConfigureServices(x =>
                {
                    var configBuilder = new ConfigurationBuilder();
                    builderAction(configBuilder);
                    var cRoot = configBuilder.Build();
                    x.AddSingleton(cRoot);
                })
                .UseStartup<Startup>();
        }
    }

    class MongoTest
    {
        private readonly IDatabaseContext _mongoContext;
        private readonly IRepository<User> _repository;

        public MongoTest(IDatabaseContext mongoContext)
        {
            _mongoContext = mongoContext;
            IRepository<User> set = _mongoContext.GetSet<User>();
            _repository = set;
            var user = new User() {Name = "Mario"};
            _repository.Add(user);
            user.Name = "Tester";
            _repository.Update(user);
            _mongoContext.SaveChanges();

            GetAll();
        }

        private async void GetAll()
        {
            var all = _repository.GetAllAsync();
            await foreach (var item in all)
            {
                Console.WriteLine(item.Name);
            }
        }

        private class User : MongoEntity
        {
            public string Name { get; set; }
        }
    }
}