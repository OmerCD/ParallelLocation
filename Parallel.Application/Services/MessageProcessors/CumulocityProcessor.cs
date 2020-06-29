using Parallel.Application.Services.Interfaces;

namespace Parallel.Application.Services.MessageProcessors
{
    public class CumulocityProcessor:IMessageProcessor<CumulocityProcessor>{
        public bool HandleConditionSatisfied(object message)
        {
            return true;
        }

        public void Handle(object message)
        {
            
        }
    }
}