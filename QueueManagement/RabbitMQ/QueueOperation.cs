using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Parallel.Shared.Credentials;
using Parallel.Shared.Helper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueManagement.RabbitMQ
{
    public class QueueOperation : IQueueOperation<BasicDeliverEventArgs>
    {
        private IConnection _connection;
        private IModel _model;
        private EventingBasicConsumer _consumer;
        private ConnectionFactory _connectionFactory;
        private readonly QueueCredential _queueCredential;

        public QueueOperation(QueueCredential queueCredential, ILogger<QueueOperation> logger = null)
        {
            _queueCredential = queueCredential;
            
        }

        public void CreateConnection()
        {
            _connectionFactory ??= new ConnectionFactory
            {
                HostName = _queueCredential.HostName,
                UserName = _queueCredential.UserName,
                Password = _queueCredential.Password,
                Endpoint = new AmqpTcpEndpoint(_queueCredential.HostName, _queueCredential.Port)
            };

            _connectionFactory.AutomaticRecoveryEnabled = true;
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            
            _consumer = new EventingBasicConsumer(_model);
            _consumer.Received += ConsumerOnReceived;
        }

        private void ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
        {
            ConsumerReceived?.Invoke(sender, e);
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
            _model?.QueueDeclare(queueName, true, false, false, null);
            _model?.QueueBind(queueName, exchangeName, routingKey);
        }

        public void DeclareQueue(string queueName)
        {
            _model?.QueueDeclare(queueName, true, false, false, null);
        }

        public EventHandler<BasicDeliverEventArgs> ConsumerReceived { get; set; }

        public IEnumerable<QueueBindingModel> GetQueueList(string exchangeName)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string requestUrl =
                    $"http://{_queueCredential.HostName}:{_queueCredential.APIPort}/api/exchanges/%2F/{exchangeName}/bindings/source";

                var token = Utility.GetBasicAuthToken(_queueCredential.UserName, _queueCredential.Password);

                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                var response = httpClient.GetAsync(requestUrl).Result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QueueBindingModel[]>(response.Result);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void SendMessageToQueue(object message, string exchangeName, string routingKey = "")
        {
            try
            {
                if (_model == null) return;
                IBasicProperties basicProperties = _model.CreateBasicProperties();
                basicProperties.Persistent = true;

                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                _model.BasicPublish(exchangeName, routingKey, basicProperties, body);
            }
            catch (Exception ex)
            {
            }
        }

        public void StartReceiving(string queueName)
        {
            string message = _model.BasicConsume(queueName, true, _consumer);
        }

        public void Dispose()
        {
            DisposeConnection();
        }
    }
}