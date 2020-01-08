namespace Parallel.Application.Services.Interfaces
{
    public interface IMessageProcessor<T> : IMessageProcessor
    {
        
    }
    public interface IMessageProcessor
    {
        bool HandleConditionSatisfied(object message);
        void Handle(object message);
    }
}