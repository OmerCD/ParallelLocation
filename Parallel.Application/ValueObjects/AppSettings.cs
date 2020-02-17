namespace Parallel.Application.ValueObjects
{
    public class AppSettings
    {
        public ConnectionInfo ConnectionInfo { get; set; }
        public ApiConnectionInfo ApiConnectionInfo { get; set; }
        public string SignalRHub { get; set; }
        public DatabaseInfo DatabaseInfo { get; set; }
        public QueueConnectInfo QueueConnectInfo { get; set; }

        public ListeningPort[] ListeningPorts { get; set; }
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
        public int Port { get; set; }
    }

    public class ListeningPort
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public byte[] StartBytes { get; set; }
        public byte[] EndBytes { get; set; }
    }
}