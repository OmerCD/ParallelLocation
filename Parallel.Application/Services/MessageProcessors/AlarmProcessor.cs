using System;
using System.Collections.Generic;
using Parallel.Application.Services.Interfaces;
using Parallel.Shared.DataTransferObjects;

namespace Parallel.Application.Services.MessageProcessors
{
    public class AlarmProcessor:IMessageProcessor<AlarmProcessor>
    {
        private readonly HashSet<ushort> _acczList = new HashSet<ushort> { 11109, 11110, 11111, 11112, 11113, 11114, 11115, 11116, 11117, 11118, 11119, 11120, 11124 };
        public bool HandleConditionSatisfied(object message)
        {
         //   return message is MessageType4 messageType4 && messageType4.Power == 8;
         return message is MessageType4 messageType4 && messageType4.Power != 8 &&_acczList.Contains(messageType4.ACCZ);
        }

        public void Handle(object message)
        {
            Console.WriteLine("\r\nAlarm Created :"+ ((dynamic)message).ACCZ);
        }
    }
}