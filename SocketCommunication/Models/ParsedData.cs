using System.Collections;
using System.Collections.Generic;
using SocketCommunication.Enums;
using SocketCommunication.Interfaces;

namespace SocketCommunication.Models
{
    public class ParsedData : IParsedData
    {
        public ParsedData()
        {
            MobileData = new List<IMobileData>();
        }

        public MessageType MessageType { get; set; }
        public bool IsOffline { get; set; }
        public byte DataCount { get; set; }
        public List<IMobileData> MobileData { get; set; }
    }
}
