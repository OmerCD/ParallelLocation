using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Parallel.Repository
{
    public class MongoContext : IDatabaseContext
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly List<Func<Task>> _commands;
        public IClientSessionHandle Session { get; set; }

        public MongoContext(string connectionString, string database)
        {
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(database);
            _commands = new List<Func<Task>>();
            BsonDefaults.GuidRepresentation = GuidRepresentation.CSharpLegacy;
            RegisterConventions();
        }

        private void RegisterConventions()
        {
            var pack = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfDefaultConvention(true)
            };
            ConventionRegistry.Register("Conventions", pack, t => true);
        }

        public IRepository<T> GetSet<T>() where T : class,IEntity
        {
            return new MongoRepositoryBase<T>(_database.GetCollection<T>(typeof(T).Name), _commands);
        }

        public async Task<int> SaveChanges()
        {
            using (Session = await _client.StartSessionAsync())
            {
                Session.StartTransaction();
                var commandTasks = _commands.Select(x => x());
                await Task.WhenAll(commandTasks);
                await Session.CommitTransactionAsync();
            }

            return _commands.Count;
        }

        public void Dispose()
        {
            while (Session != null && Session.IsInTransaction)
                Thread.Sleep(TimeSpan.FromMilliseconds(100));

            GC.SuppressFinalize(this);        }
    }
}