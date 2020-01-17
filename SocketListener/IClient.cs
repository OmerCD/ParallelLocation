using System;
using System.Net.Sockets;

namespace SocketListener
{
    public interface IClient
    {
        Socket Socket { get; set; }
        Guid SocketId { get; set; }
        bool IsConnected { get; }
    }
}