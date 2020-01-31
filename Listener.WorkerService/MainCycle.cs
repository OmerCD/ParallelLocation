using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parallel.Application.ValueObjects;
using QueueManagement;
using RabbitMQ.Client.Events;
using SocketListener;

namespace Listener.WorkerService
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly AppSettings _appSettings;
        private readonly IListener _listener;
        private bool _isListening;
        private readonly IQueueOperation<BasicDeliverEventArgs> _queueOperation;

        private readonly IDictionary<int, IClient> _clients;

        private readonly string _exchangeName;

        public MainCycle(ILogger<MainCycle> logger, AppSettings appSettings,
            IQueueOperation<BasicDeliverEventArgs> queueOperation)
        {
            _logger = logger;
            _appSettings = appSettings;
            _queueOperation = queueOperation;
            _listener = new SocketListener.Listener(_appSettings.ConnectionInfo.IsOnline);
            _clients = new ConcurrentDictionary<int, IClient>();
            _exchangeName = _appSettings.ConnectionInfo.ToString();
        }

        private void StartListening()
        {
            _clients.Clear();
            if (_isListening)
            {
                _listener.StopListener();

                _listener.ClientConnected -= ClientConnected;
                _listener.ClientDisconnected -= ClientDisconnected;

                _isListening = false;

                _queueOperation.Dispose();

                _logger.LogInformation("Stop Socket Listening");
            }
            else
            {
                _listener.ClientConnected += ClientConnected;
                _listener.ClientDisconnected += ClientDisconnected;

                _listener.StartReceive(
                    new BindInformation(_appSettings.ConnectionInfo.Port, _appSettings.ConnectionInfo.IpAddress),
                    ReceiveData);

                _isListening = true;

                _logger.LogInformation("Start Socket Listening");

                _queueOperation.CreateConnection();
                _queueOperation.DeclareQueueExchange(_exchangeName, _exchangeName);
            }
        }

        private void ClientDisconnected(IClient obj)
        {
            _clients.Remove(_clients.FirstOrDefault(x => x.Value.SocketId == obj.SocketId));
        }

        private void ClientConnected(IClient client)
        {
        }

        private void ReceiveData(byte[] received)
        {
            Console.WriteLine(string.Join(",", received));
            _queueOperation.SendMessageToQueue(received, _exchangeName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(StartListening, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(StartListening, cancellationToken);
        }
    }
}