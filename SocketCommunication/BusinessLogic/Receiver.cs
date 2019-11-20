using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketCommunication.Interfaces;
using SocketCommunication.Models;

namespace SocketCommunication.BusinessLogic
{
    public class Receiver : IReceiver
    {
        private static readonly ConcurrentDictionary<int, IClient> _clients = new ConcurrentDictionary<int, IClient>();
        private static int _clientId;

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
                    var client = server.Accept();
                    CreateClientId();
                    _clients.TryAdd(_clientId, new Client(client, _clientId));
                    while (true)
                    {
                        var receivedData = new byte[8192];
                        client.Receive(receivedData);
                        SendResponsePackageToClient(_clientId, receivedData);
                        //TODO:ReceivedData -> Database

                    }
                }
            }
        }

        private void CreateClientId()
        {
            Interlocked.Increment(ref _clientId);
        }

        public void SendResponsePackageToClient(int clientId, byte[] receivedData)
        {
            if (!_clients.ContainsKey(clientId)) return;
            var responsePackage = new byte[15];
            var list = new List<short> { 0, 1, 2, 3, 4 };
            var index = 0;
            list.ForEach(s => { responsePackage[index] = 250; index++; });
            list.ForEach(s => { responsePackage[index] = receivedData[s]; index++; });
            list.ForEach(s => { responsePackage[index] = 251; index++; });
            _clients[clientId].Socket.Send(responsePackage, 0, index, SocketFlags.None);
        }
    }
}
