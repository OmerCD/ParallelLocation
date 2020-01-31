using System;
using System.Text;
using Newtonsoft.Json;
using Parallel.Shared.Credentials;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueManagement.RabbitMQ
{
    public class QueueOperation : IQueueOperation<BasicDeliverEventArgs>
    {
        private IConnection _connection;
        private IModel _model;
        private ConnectionFactory _connectionFactory;
        private readonly QueueCredential _queueCredential;

        public QueueOperation(QueueCredential queueCredential)
        {
            _queueCredential = queueCredential;
        }

        public void CreateConnection()
        {
            _connectionFactory ??= new ConnectionFactory
            {
                HostName = _queueCredential.HostName, UserName = _queueCredential.UserName,
                Password = _queueCredential.Password
            };

            _connectionFactory.AutomaticRecoveryEnabled = true;
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
        }

        public void DisposeConnection()
        {
            if (_model != null && !_model.IsClosed)
            {
                _model.Close();
            }

            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }

            _model?.Dispose();

            _connection?.Dispose();
        }

        public void DeclareQueueExchange(string exchangeName, string queueName, string routingKey = "")
        {
            _model?.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false);
            _model?.QueueDeclare(exchangeName, true, false, false, null);
            _model?.QueueBind(queueName, exchangeName, routingKey);
        }

        public event EventHandler<BasicDeliverEventArgs> ConsumerReceived;

        public void SendMessageToQueue(object message, string exchangeName, string routingKey = "")
        {
            if (_model == null) return;
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _model.BasicPublish(exchangeName, routingKey, null, body);
        }

        public void GetMessageFromQueue(string queueName)
        {
            if (_model == null) return;
            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += ConsumerReceived;

            var message = _model.BasicConsume(queueName, true, consumer);
        }

        public void Dispose()
        {
            DisposeConnection();
        }
    }
}