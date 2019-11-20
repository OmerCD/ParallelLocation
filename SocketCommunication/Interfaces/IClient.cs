using System.Net.Sockets;

namespace SocketCommunication.Interfaces
{
    public interface IClient
    {
        Socket Socket { get; set; }
        int SocketId { get; set; }
    }
}
