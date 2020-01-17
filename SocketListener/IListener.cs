using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketListener
{
    public interface IListener
    {
        int ClientCount { get; }
        IEnumerable<IClient> ConnectedClients { get; }
        void StartReceive(BindInformation bindInformation, Action<byte[]> messageReceived);
        Action<IClient> ClientConnected { get; set; }
        Action<IClient> ClientDisconnected { get; set; }
        void StopListener();
    }
}