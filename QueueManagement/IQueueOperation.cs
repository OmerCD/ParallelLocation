using System;
using Parallel.Shared.Credentials;

namespace QueueManagement
{
    public interface IQueueOperation<T> : IDisposable where T : class
    {
        public void CreateConnection();
        public void DeclareQueueExchange(string exchangeName, string queueName, string routingKey = "");
        public void SendMessageToQueue(object message, string exchangeName, string routingKey = "");
        public void GetMessageFromQueue(string queueName);
        public event EventHandler<T> ConsumerReceived;
    }
}