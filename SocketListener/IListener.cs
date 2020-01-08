using System;

namespace SocketListener
{
    public interface IListener
    {
        int ClientCount { get; }
        void StartReceive(BindInformation bindInformation, Action<byte[]> messageReceived);
    }
}