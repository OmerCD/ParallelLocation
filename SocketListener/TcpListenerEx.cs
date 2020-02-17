using System.Net;
using System.Net.Sockets;

namespace SocketListener
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPAddress localAddress, int port) : base(localAddress, port)
        {
        }

        public TcpListenerEx(IPEndPoint localEndPoint) : base(localEndPoint)
        {
        }

        public new bool Active => base.Active;
    }
}