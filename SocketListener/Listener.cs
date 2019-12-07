using System;
using System.Net;
using System.Net.Sockets;

namespace SocketListener
{
    public class Listener : IListener
    {
        public void StartRecieve(BindInformation bindInformation, Action<byte[]> messageReceived)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(bindInformation.Address), bindInformation.Port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(1000);
            byte[] buffer = new byte[8192];

            //Gelen bağlantıyı kabul etmek için asenkron bir işlem başlatır.
            listener.BeginAccept(OnAccept, new StateInfo(listener, messageReceived, buffer));
        }

        private void OnAccept(IAsyncResult ar)
        {
            StateInfo stateInfo = (StateInfo)ar.AsyncState;
            stateInfo.Socket = stateInfo.Socket.EndAccept(ar);
            //Bağlı soketten veri akmaya başlar.
            stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive, stateInfo);
        }

        //İşlem tamamlandığında çağırılacak
        private void OnReceive(IAsyncResult ar)
        {
            int length;
            StateInfo stateInfo = (StateInfo)ar.AsyncState;
            int bytesRead = stateInfo.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                byte[] SingleMessage = new byte[0];
                byte[] received = stateInfo.Buffer[..bytesRead];
                for (int i = 0; i < received.Length; i++)
                {
                    if (received[i] == 241 && received[i + 1] == 241 && received[i + 2] == 241 && received[i + 3] == 241 && received[i + 4] == 241)
                    {
                        length = i + 5;
                        SingleMessage = new byte[length];
                        received.CopyTo(SingleMessage, 0);
                    }
                }

                stateInfo.MessageReceived(SingleMessage);
                stateInfo.Buffer = new byte[8192];
            }

            stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive, stateInfo);
        }
    }

    public struct StateInfo
    {
        public StateInfo(Socket socket, Action<byte[]> messageReceived, byte[] buffer)
        {
            Socket = socket;
            MessageReceived = messageReceived;
            Buffer = buffer;
        }

        public Socket Socket { get; set; }
        public Action<byte[]> MessageReceived { get; set; }
        public byte[] Buffer { get; set; }

    }
}
