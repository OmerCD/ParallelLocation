﻿using System;
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
    public class Listener : IListener
    {
        private readonly byte[] _startValues;
        private readonly byte[] _endValues;

        private static readonly ConcurrentDictionary<Guid, IClient> Clients = new ConcurrentDictionary<Guid, IClient>();
        private Socket _listener;

        public Listener(byte[] startValues, byte[] endValues)
        {
            _startValues = startValues;
            _endValues = endValues;
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

        public void StartReceive(BindInformation bindInformation, Action<byte[]> messageReceived)
        {
            if (bindInformation == null)
            {
                return;
            }

            var endPoint = new IPEndPoint(IPAddress.Parse(bindInformation.Address), bindInformation.Port);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endPoint);
            _listener.Listen(1000);
            Console.WriteLine($"Started Listening : {bindInformation.Address} : {bindInformation.Port}");
            var buffer = new byte[8192];

            //Gelen bağlantıyı kabul etmek için asenkron bir işlem başlatır.
            _listener.BeginAccept(OnAccept, new StateInfo(_listener, messageReceived, buffer));
            Console.WriteLine("Started Accepting Clients");
        }

        public Action<IClient> ClientConnected { get; set; }
        public Action<IClient> ClientDisconnected { get; set; }
        public void StopListener()
        {
            _listener.Close();
        }

        private void OnAccept(IAsyncResult ar)
        {
            var stateInfo = (StateInfo) ar.AsyncState;
            try
            {
                stateInfo.Socket.BeginAccept(OnAccept,
                    new StateInfo(stateInfo.Socket, stateInfo.MessageReceived, new byte[8192]));
            }
            catch (ObjectDisposedException ex)
            {
                Clients.TryRemove(stateInfo.ClientId, out _);
                return;
            }

            stateInfo.Socket = stateInfo.Socket.EndAccept(ar);
            stateInfo.ClientId = Guid.NewGuid();
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
            int length;
            StateInfo stateInfo = (StateInfo) ar.AsyncState;
            int bytesRead = 0;
            try
            {
                bytesRead = stateInfo.Socket.EndReceive(ar);
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

        private static void SendResponsePackageToClient(Guid clientId, IReadOnlyList<byte> receivedData)
        {
            if (!Clients.ContainsKey(clientId)) return;
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