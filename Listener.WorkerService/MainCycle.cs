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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly AppSettings _appSettings;
        private readonly IEnumerable<IPortListener> _portListeners;
        private readonly IQueueOperation<BasicDeliverEventArgs> _queueOperation;

        private readonly IDictionary<int, IClient> _clients;

        private const string ExchangeName = "Receiving";

        public MainCycle(ILogger<MainCycle> logger, AppSettings appSettings,
            IQueueOperation<BasicDeliverEventArgs> queueOperation, IEnumerable<IPortListener> portListeners)
        {
            _logger = logger;
            _appSettings = appSettings;
            _queueOperation = queueOperation;
            _portListeners = portListeners;
            _clients = new ConcurrentDictionary<int, IClient>();
            try
            {

                _queueOperation.CreateConnection();
                foreach (var item in _portListeners)
                {
                    _queueOperation.DeclareQueueExchange(ExchangeName, item.Name, item.Name + "Route");
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Couldn't start Listener");
                throw;
            }
        }

        private void StartListening()
        {
            _clients.Clear();

            foreach (IPortListener item in _portListeners)
            {
                if (item.IsListening)
                {
                    item.StopListening();

                    item.ClientConnected -= ClientConnected;
                    item.ClientDisconnected -= ClientDisconnected;

                    _queueOperation.Dispose();

                    _logger.LogInformation("Stop Socket Listening");
                }
                else
                {
                    item.ClientConnected += ClientConnected;
                    item.ClientDisconnected += ClientDisconnected;

                    item.PackageCompleted = ReceiveData;

                    item.StartListening();

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
            DateTime dateNow = DateTime.Now;
            Console.WriteLine($"{dateNow} - {string.Join(",", received)}");

            var packetFromQueue = new PacketFromQueue
            {
                Buffer = received,
                ReceiveDate = dateNow,
                QueueDate = dateNow,
            };
            //TODO : Detect alarms and send to AlarmQueue
            _queueOperation.SendMessageToQueue(packetFromQueue, ExchangeName, portListener.Name + "Route");
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