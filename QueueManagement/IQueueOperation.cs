using System;
using System.Collections.Generic;
using Parallel.Shared.Credentials;

namespace QueueManagement
{
    public interface IQueueOperation<T> : IDisposable where T : class
    {
        void CreateConnection();
        void DeclareQueueExchange(string exchangeName, string queueName, string routingKey = "");
        void DeclareQueue(string queueName);

        void SendMessageToQueue(object message, string exchangeName, string routingKey = "");
        void StartReceiving(string queueName);
        EventHandler<T> ConsumerReceived { get; set; }
        IEnumerable<QueueBindingModel> GetQueueList(string exchangeName);
    }
}