using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Application.ValueObjects;
using SocketListener;
using SocketListener.Extensions;

namespace Listener.WorkerService
{
    public class TcpPortListener : IPortListener
    {
        private TcpListenerEx _listener;
        private readonly byte[] _startValues;
        private readonly byte[] _endValues;
        private readonly int _port;
        private readonly string _ipAddress;
        private static readonly ConcurrentDictionary<Guid, IClient> Clients = new ConcurrentDictionary<Guid, IClient>();

        public Action<IPortListener, byte[]> PackageCompleted { get; set; }
        public string Name { get; }

        public void StartListening()
        {
            StartReceive(
                new BindInformation(_port, _ipAddress),
                PackageCompleted);
            IsListening = true;
        }

        public void StopListening()
        {
            foreach ((Guid _, IClient client) in Clients)
            {
                if (client.Socket.Connected)
                {
                    client.Socket.Shutdown(SocketShutdown.Both);
                    client.Socket.Close();
                }
            }

            IsListening = false;
            _listener.Stop();
            Clients.Clear();
        }


        public TcpPortListener(byte[] startValues, byte[] endValues, string name, int port, string ipAddress)
        {
            _startValues = startValues;
            _endValues = endValues;
            _port = port;
            _ipAddress = ipAddress;
            Name = name;
        }

        public int ClientCount
        {
            get
            {
                RemoveDisconnectedClients();
                return Clients.Count;
            }
        }

        public IEnumerable<IClient> ConnectedClients
        {
            get
            {
                RemoveDisconnectedClients();
                return Clients.Values;
            }
        }

        public bool IsListening { get; set; }

        private void RemoveDisconnectedClients()
        {
            IEnumerable<Guid> idList = Clients.Where(client => !client.Value.IsConnected).Select(client => client.Key);

            foreach (Guid guid in idList)
            {
                if (Clients.TryRemove(guid, out var client))
                {
                    ClientDisconnected?.Invoke(client);
                }
            }
        }

        private void StartReceive(BindInformation bindInformation, Action<IPortListener, byte[]> messageReceived)
        {
            if (bindInformation == null)
            {
                return;
            }

            var endPoint = new IPEndPoint(IPAddress.Parse(bindInformation.Address), bindInformation.Port);
            _listener = new TcpListenerEx(endPoint);
            _listener.Start();
            Console.WriteLine($"Started Listening : {bindInformation.Address} : {bindInformation.Port}");
            var buffer = new byte[8192];

            //Gelen bağlantıyı kabul etmek için asenkron bir işlem başlatır.
            _listener.BeginAcceptSocket(OnAccept, Tuple.Create(_listener, messageReceived));
            Console.WriteLine("Started Accepting Clients");
        }

        public Action<IClient> ClientConnected { get; set; }
        public Action<IClient> ClientDisconnected { get; set; }


        private void OnAccept(IAsyncResult ar)
        {
            var ( tcpListener, messageReceived) = (Tuple<TcpListenerEx, Action<IPortListener, byte[]>>) ar.AsyncState;
            try
            {
                if (tcpListener.Active)
                {
                    tcpListener.BeginAcceptSocket(OnAccept, Tuple.Create(tcpListener, messageReceived));
                }
                else return;
            }
            catch (ObjectDisposedException ex)
            {
                // Clients.TryRemove(stateInfo.ClientId, out _);
                return;
            }

            var stateInfo = new StateInfo();
            stateInfo.Socket = tcpListener.EndAcceptSocket(ar);
            stateInfo.ClientId = Guid.NewGuid();
            stateInfo.Buffer = new byte[8192];
            stateInfo.MessageReceived = messageReceived;
            var client = new Client(stateInfo.Socket, stateInfo.ClientId);
            if (Clients.TryAdd(stateInfo.ClientId, client))
            {
                ClientConnected?.Invoke(client);
            }


            //Bağlı soketten veri akmaya başlar.
            stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive,
                stateInfo);
        }

        //İşlem tamamlandığında çağırılacak
        private async void OnReceive(IAsyncResult ar)
        {
            StateInfo stateInfo = (StateInfo) ar.AsyncState;
            int bytesRead = 0;
            try
            {
                if (stateInfo.Socket.Connected)
                {
                    bytesRead = stateInfo.Socket.EndReceive(ar);
                }
            }
            catch (SocketException ex)
            {
                if (Clients.TryRemove(stateInfo.ClientId, out var client))
                {
                    ClientDisconnected?.Invoke(client);
                }

                return;
            }

            if (bytesRead > 0)
            {
                byte[] received = stateInfo.Buffer[..bytesRead];
                SendResponsePackageToClient(stateInfo.ClientId, stateInfo.Buffer);
                if (stateInfo.LastPackageBuffer?.Length > 0)
                {
                    received = stateInfo.LastPackageBuffer.Concat(received).ToArray();
                    stateInfo.LastPackageBuffer = Array.Empty<byte>();
                }

                IAsyncEnumerable<PackageInfo> packages = GetPackages(received);

                await foreach (PackageInfo package in packages)
                {
                    if (package.IsFinished)
                    {
                        stateInfo.MessageReceived(this, package.Bytes);
                    }
                    else
                    {
                        stateInfo.LastPackageBuffer = package.Bytes;
                    }
                }

                stateInfo.Buffer = new byte[8192];
            }

            if (stateInfo.Socket.Connected)
            {
                stateInfo.Socket.BeginReceive(stateInfo.Buffer, 0, stateInfo.Buffer.Length, SocketFlags.None, OnReceive,
                    stateInfo);
            }
        }

        private static void SendResponsePackageToClient(Guid clientId, IReadOnlyList<byte> receivedData)
        {
            if (!Clients.ContainsKey(clientId)) return;
            if (!Clients[clientId].IsConnected) return;

            var responsePackage = new byte[15];
            var list = new List<byte> {0, 1, 2, 3, 4};
            var index = 0;
            list.ForEach(s =>
            {
                responsePackage[index] = 250;
                index++;
            });
            list.ForEach(s =>
            {
                responsePackage[index] = receivedData[s];
                index++;
            });
            list.ForEach(s =>
            {
                responsePackage[index] = 251;
                index++;
            });
            Clients[clientId].Socket.Send(responsePackage, 0, index, SocketFlags.None);
        }

        private async IAsyncEnumerable<PackageInfo> GetPackages(byte[] received)
        {
            var lastIndex = 0;

            while (lastIndex < received.Length)
            {
                int first;
                var last = 0;

                try
                {
                    int tempValLast = lastIndex;
                    first = await Task.Run(() => received.IndexOf(_startValues, tempValLast)).ConfigureAwait(false);
                    int tempValFirst = first;
                    last = await Task.Run(() => received.IndexOf(_endValues, tempValFirst)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (lastIndex > 0 && first == -1)
                {
                    yield return new PackageInfo(received[lastIndex..], false);
                    yield break;
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

                first += _startValues.Length;
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
                lastIndex = last + _endValues.Length;
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
        public StateInfo(Socket socket, Action<IPortListener, byte[]> messageReceived, byte[] buffer)
        {
            Socket = socket;
            MessageReceived = messageReceived;
            Buffer = buffer;
            PackageStarted = false;
            LastPackageBuffer = Array.Empty<byte>();
            ClientId = new Guid();
        }

        public Socket Socket { get; set; }
        public Action<IPortListener, byte[]> MessageReceived { get; set; }
        public byte[] Buffer { get; set; }
        public bool PackageStarted { get; set; }
        public byte[] LastPackageBuffer { get; set; }
        public Guid ClientId { get; set; }
    }
}