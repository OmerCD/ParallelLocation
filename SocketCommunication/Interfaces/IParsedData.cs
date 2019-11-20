using System.Collections;
using System.Collections.Generic;
using SocketCommunication.Enums;

namespace SocketCommunication.Interfaces
{
    public interface IParsedData
    {
        MessageType MessageType { get; set; }
        bool IsOffline { get; set; }
        byte DataCount { get; set; }
        List<IMobileData> MobileData { get; set; }
    }
}
