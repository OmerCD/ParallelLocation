namespace Parallel.Shared.Credentials
{
    public class QueueCredential
    {
        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
        public int Port { get; set; }

        public QueueCredential(string hostName = "localhost", string userName = "guest", string password = "guest", int port=15672)
        {
            HostName = hostName;
            UserName = userName;
            Password = password;
            Port = port;
        }
    }
}