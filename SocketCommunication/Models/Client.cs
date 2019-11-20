using System.Net.Sockets;
using SocketCommunication.Interfaces;

namespace SocketCommunication.Models
{
    public class Client : IClient
    {
        public Client(Socket socket, int socketId)
        {
            Socket = socket;
            SocketId = socketId;
        }

        public Socket Socket { get; set; }
        public int SocketId { get; set; }
    }
}
