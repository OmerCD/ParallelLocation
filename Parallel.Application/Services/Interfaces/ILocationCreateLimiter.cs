using System;

namespace Parallel.Application.Services.Interfaces
{
    public interface ILocationCreateLimiter<TMessage>
    {
        Action<TMessage[]> LocationReady { get; set; }
        void AddLocationMessage(TMessage message);
    }
}