using Parallel.Application.Services.Interfaces;
using Parallel.Shared.DataTransferObjects;

namespace Parallel.Application.Services.MessageProcessors
{
    public class CanBusProcessor : IMessageProcessor<CanBusProcessor>
    {
        public bool HandleConditionSatisfied(object message)
        {
            if (message is CanBusInfo)
            {
                return true;
            }
            return false;
            throw new System.NotImplementedException();
        }

        public void Handle(object message)
        {
            if (!HandleConditionSatisfied(message))
            {
                return;
            }
            throw new System.NotImplementedException();
        }
    }
}