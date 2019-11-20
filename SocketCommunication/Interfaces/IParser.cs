namespace SocketCommunication.Interfaces
{
    public interface IParser
    {
        bool Parse(byte[] receivedData, int receivedDataLength, out IParsedData parsedData);
    }

}
