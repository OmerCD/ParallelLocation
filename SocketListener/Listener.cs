using SocketListener.Utilities;
using System;
using System.Net;
using System.Net.Sockets;

namespace SocketListener
{
    public class Listener : IListener
    {
        private readonly BoyerMoore _startPattern;
        private readonly BoyerMoore _endPattern;

        public Listener()
        {
            _startPattern = new BoyerMoore(new byte[] { 240, 240, 240, 240, 240 });
            _endPattern = new BoyerMoore(new byte[] { 241, 241, 241, 241, 241 });
        }

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
                byte[] received = stateInfo.Buffer[..bytesRead];
                var start = _startPattern.Search(received);
                start += _startPattern.PatternLength;
                var end = _endPattern.Search(received, start);
                stateInfo.MessageReceived(received[start..end]);
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
