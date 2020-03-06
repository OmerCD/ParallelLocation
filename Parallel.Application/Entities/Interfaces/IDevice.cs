using System;

namespace Parallel.Application.Entities.Interfaces
{
    public interface IDevice<T>
    {
        public T Id { get; set; }
    }

    public interface IRfDevice : IDevice<int>
    {
        public DateTime LastOnlineDate { get; set; }
    }
}