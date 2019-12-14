using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parallel.Location;
using Parallel.Shared.DataTransferObjects;
using ReflectorO;
using SocketCommunication.Interfaces;
using SocketCommunication.Models;
using SocketListener;

namespace Parallel.Main
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IConfiguration _configuration;
        private readonly IElector _elector;
        private readonly IReceiver _receiver;
        private readonly IParseRouter<byte[]> _parseRouter;

        public MainCycle(ILogger<MainCycle> logger, IConfiguration configuration, IElector elector, IReceiver receiver,
            IParseRouter<byte[]> parseRouter, ILocationCalculatorRouter<Type> locationCalculatorRouter)
        {
            _logger = logger;
            _configuration = configuration;
            _elector = elector;
            _receiver = receiver;
            _parseRouter = parseRouter;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new Listener();

            listener.StartReceive(new BindInformation(5252, "192.168.10.41"), ReceiveData);
            return Task.FromResult(0);
        }

        private void ReceiveData(byte[] received)
        {
            object message = _parseRouter.GetObject(received);
            if (message != null)
            {
               
                    Console.WriteLine($"{ObjectDumper.Dump(message)}");
                
            }
        }
    }
}