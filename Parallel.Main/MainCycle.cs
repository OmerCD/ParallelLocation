using System;
using System.Threading;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parallel.Location;
using ReflectorO;
using SocketCommunication.Interfaces;
using SocketCommunication.Models;

namespace Parallel.Main
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IConfiguration _configuration;
        private readonly IElector _elector;
        private readonly IReceiver _receiver;

        public MainCycle(ILogger<MainCycle> logger, IConfiguration configuration, IElector elector, IReceiver receiver, IParseRouter<byte[]> parseRouter, ILocationCalculatorRouter<Type> locationCalculatorRouter)
        {
            _logger = logger;
            _configuration = configuration;
            _elector = elector;
            _receiver = receiver;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var socketInfo = new SocketInformation
            {
                Ip = "192.168.10.41",
                Port = "424242"
            };
            _receiver.Receive(socketInfo);
            return Task.FromResult(0);
        }

        private void ReceiveData(byte[] received)
        {
            
        }
    }
}