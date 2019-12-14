using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SocketListener.Extensions;

namespace SocketListener
{
    public interface IClient
    {
        Socket Socket { get; set; }
        Guid SocketId { get; set; }
    }
    public class Client : IClient
    {
        public Client(Socket socket, Guid socketId)
        {
            Socket = socket;
            SocketId = socketId;
        }

        public Socket Socket { get; set; }
        public Guid SocketId { get; set; }
    }
    public class Listener : IListener
    {
        private static readonly byte[] StartValues = new byte[] {240, 240, 240, 240, 240};
        private static readonly byte[] EndValues = new byte[] {241, 241, 241, 241, 241};
        
        private static readonly ConcurrentDictionary<Guid, IClient> _clients = new ConcurrentDictionary<Guid, IClient>();

        public void StartReceive(BindInformation bindInformation, Action<byte[]> messageReceived)
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
            StateInfo stateInfo = (StateInfo) ar.AsyncState;
            stateInfo.Socket.BeginAccept(OnAccept, new StateInfo(stateInfo.Socket, stateInfo.MessageReceived, new byte[8192]));
            
            stateInfo.Socket = stateInfo.Socket.EndAccept(ar);
            stateInfo.ClientId = Guid.NewGuid();
            _clients.TryAdd(stateInfo.ClientId, new Client(stateInfo.Socket, stateInfo.ClientId));

            
            //Bağlı soketten veri akmaya başlar.
            stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive,
                stateInfo);
            
        }

        //İşlem tamamlandığında çağırılacak
        private async void OnReceive(IAsyncResult ar)
        {
            int length;
            StateInfo stateInfo = (StateInfo) ar.AsyncState;
            int bytesRead = stateInfo.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                byte[] received = stateInfo.Buffer[..bytesRead];
                SendResponsePackageToClient(stateInfo.ClientId, stateInfo.Buffer);
                if (stateInfo.LastPackageBuffer?.Length > 0)
                {
                    received = stateInfo.LastPackageBuffer.Concat(received).ToArray();
                    stateInfo.LastPackageBuffer = Array.Empty<byte>();
                }
                var packages = GetPackages(received);
                
                await foreach (var package in packages)
                {
                    if (package.IsFinished)
                    {
                        stateInfo.MessageReceived(package.Bytes);
                    }
                    else
                    {
                        stateInfo.LastPackageBuffer = package.Bytes;
                    }
                }

                stateInfo.Buffer = new byte[8192];
            }

            stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive,
                stateInfo);
        }
        private static void SendResponsePackageToClient(Guid clientId, byte[] receivedData)
        {
            if (!_clients.ContainsKey(clientId)) return;
            var responsePackage = new byte[15];
            var list = new List<byte> { 0, 1, 2, 3, 4 };
            var index = 0;
            list.ForEach(s => { responsePackage[index] = 250; index++; });
            list.ForEach(s => { responsePackage[index] = receivedData[s]; index++; });
            list.ForEach(s => { responsePackage[index] = 251; index++; });
            _clients[clientId].Socket.Send(responsePackage, 0, index, SocketFlags.None);
        }
        private async IAsyncEnumerable<PackageInfo> GetPackages(byte[] received)
        {
            var lastIndex = 0;

            int IndexOfStart()
            {
                return received.IndexOf(StartValues, lastIndex);
            }

            while (lastIndex < received.Length)
            {
                int first;
                int last=0;

                try
                {
                    first = await Task.Run(IndexOfStart);
                    
                    last = await Task.Run(() => received.IndexOf(EndValues, first));
                 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                if (last == -1)
                {
                    if (first < 0 || first >= received.Length)
                    {
                        yield return new PackageInfo(received, false);
                    }
                    else
                    {
                        yield return new PackageInfo(received[first..], false);
                    }

                    yield break;
                }
                first += StartValues.Length;
                if (first > last)
                {
                    break;
                }

                PackageInfo packageInfo = default;
                try
                {
                    packageInfo = new PackageInfo(received[first..last], true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                yield return packageInfo;
                lastIndex = last + EndValues.Length;
            }
        }
    }

    public struct PackageInfo
    {
        public PackageInfo(byte[] bytes, bool isFinished)
        {
            Bytes = bytes;
            IsFinished = isFinished;
        }

        public byte[] Bytes { get; set; }
        public bool IsFinished { get; set; }
    }

    public struct StateInfo
    {
        public StateInfo(Socket socket, Action<byte[]> messageReceived, byte[] buffer)
        {
            Socket = socket;
            MessageReceived = messageReceived;
            Buffer = buffer;
            PackageStarted = false;
            LastPackageBuffer = Array.Empty<byte>();
            ClientId = new Guid();
        }

        public Socket Socket { get; set; }
        public Action<byte[]> MessageReceived { get; set; }
        public byte[] Buffer { get; set; }
        public bool PackageStarted { get; set; }
        public byte[] LastPackageBuffer { get; set; }
        public Guid ClientId { get; set; }
    }
}