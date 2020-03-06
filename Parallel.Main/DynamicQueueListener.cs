using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Logging;
using Parallel.Application.ValueObjects;
using QueueManagement;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Parallel.Application.Services.Interfaces;
using Parallel.Shared.Credentials;
using Parallel.Shared.Helper;
using Parallel.Shared.PacketObjects;

namespace Parallel.Main
{
    public class DynamicQueueListener
    {
        private readonly ILogger<DynamicQueueListener> _logger;
        private readonly AppSettings _appSettings;
        private readonly IParseRouter<byte[]> _parseRouter;
        private readonly IProcessManager _processManager;

        private readonly IQueueOperation<BasicDeliverEventArgs> _queueOperation;

        private readonly IEnumerable<QueueBindingModel> _queueBindingModels;
        
        public DynamicQueueListener(ILogger<DynamicQueueListener> logger, AppSettings appSettings,
            IQueueOperation<BasicDeliverEventArgs> queueOperation, IParseRouter<byte[]> parseRouter, IProcessManager processManager)
        {
            _logger = logger;
            _appSettings = appSettings;
            _queueOperation = queueOperation;
            _parseRouter = parseRouter;
            _processManager = processManager;

            try
            {
                _queueOperation.CreateConnection();
                _queueBindingModels = _queueOperation.GetQueueList("Receiving");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,"Couldn't start DynamicQueueListener");
                throw;
            }
        }

        public void StartListeningQueues()
        {
            _queueOperation.ConsumerReceived = ConsumerReceived;
            
            foreach (var item in _queueBindingModels)
            {
                _queueOperation.StartReceiving(item.Destination);
            }
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            // _queueList.Add(e);
            // Console.WriteLine(e.RoutingKey);
            //
            // Console.WriteLine(ObjectDumper.Dump(JsonConvert.DeserializeObject<PacketFromQueue>(Encoding.UTF8.GetString(e.Body))));

            var packet = JsonConvert.DeserializeObject<PacketFromQueue>(Encoding.UTF8.GetString(e.Body));
            
            var message = _parseRouter.GetObject(packet.Buffer);
            if (message != null)
            {
                _processManager.Handle(message);
            }
        }
    }
}