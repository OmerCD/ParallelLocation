namespace Parallel.Application.Services.Interfaces
{
    public interface IProcessManager
    {
        void Handle(object handlingObject);
        void RegisterProcess(IMessageProcessor messageProcessor);
    }
}