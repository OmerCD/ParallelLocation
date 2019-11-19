using SocketCommunication.Interfaces;

namespace SocketCommunication.Models
{
    public class SocketInformation : ISocketInformation
    {
        public string Ip { get; set; }
        public string Port { get; set; }
    }
}
