namespace SocketListener
{
    public class BindInformation
    {
        public BindInformation(int port, string address)
        {
            Port = port;
            Address = address;
        }

        public int Port { get; internal set; }
        public string Address { get; internal set; }
    }
}