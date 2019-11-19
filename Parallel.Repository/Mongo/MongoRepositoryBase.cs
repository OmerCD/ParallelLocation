using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Parallel.Repository
{
    public class MongoRepositoryBase<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly IMongoCollection<T> Collection;
        protected readonly List<Func<Task>> Commands;

        public MongoRepositoryBase(IMongoCollection<T> collection, List<Func<Task>> commands)
        {
            Collection = collection;
            Commands = commands;
        }

        private static bool CheckKey(out ObjectId id, params object[] key)
        {
            id = ObjectId.Empty;
            if (key != null && key.Length > 0)
            {
                try
                {
                    id = (ObjectId) key[0];
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        private static void GenerateId(IEntity item)
        {
            if (item is IEntity<ObjectId> entity)
            {
                if (entity.GetId() == default)
                {
                    entity.SetId(ObjectId.GenerateNewId());
                }
            }
            else
            {
                throw new MongoException($"Wrong entity type. Expected {nameof(IEntity)}<{nameof(ObjectId)}>");
            }
        }

        public virtual async IAsyncEnumerable<T> GetAllAsync()
        {
            var cursor = await Collection.FindAsync(new BsonDocument());
            await cursor.MoveNextAsync();
            foreach (T item in cursor.Current)
            {
                yield return item;
            }
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Collection.Find(new BsonDocument()).ToEnumerable();
        }

        public virtual T GetByKey(params object[] key)
        {
            if (CheckKey(out var id, key))
            {
                return Collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefault();
            }

            throw new MongoException($"Couldn't convert given key to the {nameof(ObjectId)}");
        }

        public virtual Task<T> GetByKeyAsync(params object[] key)
        {
            return Task.Run(() => GetByKey(key));
        }

        public virtual T Add(T item)
        {
            GenerateId(item);
            AddCommand(async () => Collection.InsertOne(item));
            return item;
        }

        public virtual Task<T> AddAsync(T item)
        {
            GenerateId(item);
            AddCommand(async () => await Collection.InsertOneAsync(item));
            return Task.FromResult(item);
        }

        public virtual T Update(T updatedItem)
        {
            if (updatedItem is IEntity<ObjectId> entity)
            {
                AddCommand(async () =>
                {
                    var oldEntity = GetByKey(entity.Id);
                    var updateStatements = CheckAndCreateUpdateDefinitions(oldEntity, updatedItem);
                    var updateDefinitions = updateStatements as UpdateDefinition<T>[] ?? updateStatements.ToArray();
                    if (updateDefinitions.Any())
                    {
                        Collection.UpdateOne(Builders<T>.Filter.Eq("_id", entity.Id),
                            Builders<T>.Update.Combine(updateDefinitions), new UpdateOptions {IsUpsert = true});
                    }
                });
            }
            else
            {
                throw new MongoException($"Wrong entity type. Expected {nameof(IEntity)}<{nameof(ObjectId)}>");
            }

            return updatedItem;
        }

        private IEnumerable<UpdateDefinition<T>> CheckAndCreateUpdateDefinitions(T oldItem,
            T newItem)
        {
            var properties = typeof(T).GetProperties();
            for (var i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name != "Id")
                {
                    object oldValue = properties[i].GetValue(oldItem);
                    object newValue = properties[i].GetValue(newItem);
                    if (!oldValue.Equals(newValue))
                    {
                        var statement = Builders<T>.Update.Set(properties[i].Name, newValue);
                        yield return statement;
                    }
                }
            }
        }

        public virtual Task<T> UpdateAsync(T updatedItem)
        {
            return Task.Run(() => Update(updatedItem));
        }

        private Task AddCommandAsync(Func<Task> func)
        {
            return Task.Run(() => Commands.Add(func));
        }

        private void AddCommand(Func<Task> func)
        {
            Commands.Add(func);
        }
    }
}