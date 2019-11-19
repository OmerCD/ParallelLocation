using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Servers;

namespace Parallel.Repository
{
    public interface IEntity
    {
        object Id { get; set; }
    }
    public interface IEntity<T> : IEntity
    {
        T GetId()
        {
            if (Id != null)
            {
                return (T) Convert.ChangeType(Id, typeof(T));
            }
            else
            {
                return default;
            }
        }

        void SetId(T id)
        {
            Id = id;
        }
    }

    public abstract class SQLEntity : IEntity<int>
    {
        [Key] public object Id { get; set; }
    }

    public abstract class MongoEntity : IEntity<ObjectId>
    {
        [BsonId] public object Id { get; set; }
    }
}