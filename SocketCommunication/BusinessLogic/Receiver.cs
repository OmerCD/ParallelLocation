using System;
using System.Net;
using System.Net.Sockets;
using SocketCommunication.Interfaces;

namespace SocketCommunication.BusinessLogic
{
    public class Receiver : IReceiver
    {
        public void Receive(ISocketInformation socketInformation)
        {
            //if (!Utility.CheckIpIsCorrectFormat(socketInformation.Ip) || !Utility.CheckPortIsCorrectFormat(socketInformation.Port)) throw new SocketException();

            var iep = new IPEndPoint(IPAddress.Parse(socketInformation.Ip), Convert.ToInt32(socketInformation.Port));

            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(iep);
                server.Listen(250);

                while (true)
                {
                    var buffer = new byte[8192];
                    server.Accept().Receive(buffer);
                }
            }
        }
    }
}
