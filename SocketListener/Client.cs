using System;
using System.Net.Sockets;

namespace SocketListener
{
    public class Client : IClient
    {
        public Client(Socket socket, Guid socketId)
        {
            Socket = socket;
            SocketId = socketId;
        }

        public Socket Socket { get; set; }
        public Guid SocketId { get; set; }
        public bool IsConnected
        {
            get
            {
                bool part1 = Socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (Socket.Available == 0);
                return !part1 || !part2;
            }
        }
    }
}