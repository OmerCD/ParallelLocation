using Parallel.Repository;

namespace Parallel.Application.Entities.Database.Mongo
{
    public class LocationRecord : MongoEntity
    {
        public double X { get; set; }
        public double Z { get; set; }
        public double Y { get; set; }
        public int TagId { get; set; }
        public MessageRecord[] MessageRecords { get; set; }
    }
}