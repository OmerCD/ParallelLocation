namespace Parallel.Application.ValueObjects
{
    public class AppSettings
    {
        public ConnectionInfo ConnectionInfo { get; set; }
        public ApiConnectionInfo ApiConnectionInfo { get; set; }
        public string SignalRHub { get; set; }
        public DatabaseInfo DatabaseInfo { get; set; }

        public QueueConnectInfo QueueConnectInfo { get; set; }
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
        public bool IsOnline { get; set; }

        public override string ToString()
        {
            return IsOnline ? "OnlineMessages" : "OfflineMessages";
        }
    }

    public class ApiConnectionInfo
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }

    public class QueueConnectInfo
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}