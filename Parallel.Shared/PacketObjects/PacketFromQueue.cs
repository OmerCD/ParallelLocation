using System;

namespace Parallel.Shared.PacketObjects
{
    public class PacketFromQueue
    {
        public DateTime ReceiveDate { get; set; }
        public DateTime QueueDate { get; set; }
        public DateTime? ReQueueDate { get; set; }
        public bool IsOnline { get; set; }
        public byte[] Buffer { get; set; }
    }
}