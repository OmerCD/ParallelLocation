namespace Parallel.Main.ValueObjects
{
    public class AppSettings
    {
        public ConnectionInfo ConnectionInfo { get; set; }
        public DatabaseInfo DatabaseInfo { get; set; }
    }

    public class DatabaseInfo
    {
        public MongoDatabase MongoDatabase { get; set; }
    }

    public class MongoDatabase
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class ConnectionInfo
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}