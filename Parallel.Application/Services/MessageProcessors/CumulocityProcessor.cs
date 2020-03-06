using Parallel.Application.Services.Interfaces;

namespace Parallel.Application.Services.MessageProcessors
{
    public class CumulocityProcessor:IMessageProcessor<CumulocityProcessor>{
        public bool HandleConditionSatisfied(object message)
        {
            return false;
        }

        public void Handle(object message)
        {
            
        }
    }
}