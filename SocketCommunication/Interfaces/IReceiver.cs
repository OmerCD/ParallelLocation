namespace SocketCommunication.Interfaces
{
    public interface IReceiver
    {
        void Receive(ISocketInformation socketInformation);
        void SendResponsePackageToClient(int clientId, byte[] receivedData);
    }
}
