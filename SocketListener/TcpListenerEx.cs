using System.Net;
using System.Net.Sockets;

namespace SocketListener
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(int port) : base(port)
        {
        }

        public TcpListenerEx(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        public TcpListenerEx(IPEndPoint localEP) : base(localEP)
        {
        }

        public new bool Active => base.Active;
    }
}