using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketCommunication.BusinessLogic;
using SocketCommunication.Enums;
using SocketCommunication.Interfaces;
using SocketCommunication.Models;
using SocketInformation = SocketCommunication.Models.SocketInformation;

namespace BusinessTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var receiver = new Receiver();
                receiver.Receive(new SocketInformation
                {
                    Ip = "192.168.10.41",
                    Port = "5252"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();

        }

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
                            var receivedDataLength = client.Receive(receivedData);
                            //TODO:ReceivedData -> Database
                            SendResponsePackageToClient(_clientId, receivedData);
                            
                            if (new Parser().Parse(receivedData, receivedDataLength, out var parsedData))
                            {
                                parsedData.MobileData.Where(s => s.Power == 8).ToList().ForEach(s => Console.WriteLine($"distance: {s.Distance}, readernodeid:{s.ReaderNodeId}"));
                            }
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
}
