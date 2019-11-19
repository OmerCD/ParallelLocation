using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Parallel.Location;
using Parallel.Repository;

namespace Parallel.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var serviceCollection = new ServiceCollection();
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
#if DEBUG
                .AddJsonFile("appsettings.Development.json", true, true)
#else
                .AddJsonFile($"appsettings.json", true, true);
#endif
                .AddJsonFile($"appsettings.{environment}.json", true, true);
            IConfigurationRoot configurationRoot = builder.Build();
            
            var startup = new Startup(configurationRoot);
            startup.ConfigureServices(serviceCollection);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var mT = new MongoTest(scope.ServiceProvider.GetService<IDatabaseContext>());

            Console.ReadKey();
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