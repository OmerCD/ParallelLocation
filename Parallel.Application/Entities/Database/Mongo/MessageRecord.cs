using Parallel.Repository;

namespace Parallel.Application.Entities.Database.Mongo
{
    public class MessageRecord : MongoEntity
    {
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public object DataObject { get; set; }
    }
}