namespace Parallel.Application.ValueObjects
{
    public class AppSettings
    {
        public ConnectionInfo ConnectionInfo { get; set; }
        public ApiConnectionInfo ApiConnectionInfo { get; set; }
        public string SignalRHub { get; set; }
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

    public class ApiConnectionInfo
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}