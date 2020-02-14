using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Parallel.Application.ValueObjects;
using Parallel.Shared.PacketObjects;
using QueueManagement;
using RabbitMQ.Client.Events;
using SocketListener;

namespace Listener.WorkerService
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly AppSettings _appSettings;
        private readonly List<IPortListener> _portListener;
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
            _clients = new ConcurrentDictionary<int, IClient>();
            _exchangeName ="Receiving";

            _queueOperation.CreateConnection();
            _portListener = new List<IPortListener>();
            foreach (var item in _appSettings.ListeningPorts)
            {
                var tcpPortListener = new TcpPortListener(item.StartBytes, item.EndBytes, item.Name, item.Port,
                    _appSettings.ConnectionInfo.IpAddress);

                tcpPortListener.PackageCompleted = ReceiveData;
                
                _portListener.Add(tcpPortListener);

                _queueOperation.DeclareQueueExchange(_exchangeName, item.Name, item.Name + "Route");
            }
        }

        private void StartListening()
        {
            _clients.Clear();

            foreach (var item in _portListener)
            {
                if (_isListening)
                {
                    item.StopListening();

                    item.ClientConnected -= ClientConnected;
                    item.ClientDisconnected -= ClientDisconnected;

                    _isListening = false;

                    _queueOperation.Dispose();

                    _logger.LogInformation("Stop Socket Listening");
                }
                else
                {
                    item.ClientConnected += ClientConnected;
                    item.ClientDisconnected += ClientDisconnected;

                    item.StartListening();

                    _isListening = true;

                    _logger.LogInformation("Start Socket Listening");
                }
            }
        }

        private void ClientDisconnected(IClient obj)
        {
            _clients.Remove(_clients.FirstOrDefault(x => x.Value.SocketId == obj.SocketId));
        }

        private void ClientConnected(IClient client)
        {
        }

        private void ReceiveData(IPortListener portListener, byte[] received)
        {
            var date = DateTime.Now;
            Console.WriteLine($"{date} - {string.Join(",", received)}");

            PacketFromQueue packetFromQueue = new PacketFromQueue
            {
                Buffer = received,
                ReceiveDate = date,
                QueueDate = date,
                IsOnline = _appSettings.ConnectionInfo.IsOnline
            };

            _queueOperation.SendMessageToQueue(packetFromQueue, _exchangeName, portListener.Name + "Route");
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