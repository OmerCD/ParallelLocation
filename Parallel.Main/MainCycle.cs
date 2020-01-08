#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parallel.Application.Entities.Database.Mongo;
using Parallel.Application.Services.Interfaces;
using Parallel.Location;
using Parallel.Main.ValueObjects;
using Parallel.Repository;
using Parallel.Shared.DataTransferObjects;
using SocketCommunication.Models;
using SocketListener;

namespace Parallel.Main
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IParseRouter<byte[]> _parseRouter;
        private readonly AppSettings _appSettings;
        private readonly IProcessManager _processManager;
        private readonly IListener _listener;


        public MainCycle(ILogger<MainCycle> logger,
            IParseRouter<byte[]> parseRouter, AppSettings appSettings, IProcessManager processManager)
        {
            _logger = logger;
            _parseRouter = parseRouter;
            _appSettings = appSettings;
            _processManager = processManager;
            _listener = new Listener(new byte[] {240, 240, 240, 240, 240}, new byte[] {241, 241, 241, 241, 241});
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.StartReceive(
                new BindInformation(_appSettings.ConnectionInfo.Port, _appSettings.ConnectionInfo.IpAddress),
                ReceiveData);
            Fancy();
            return Task.FromResult(0);
        }

        private async void Fancy()
        {
            while (true)
            {
                Console.Write("\rClient Count :" + _listener.ClientCount);
                await Task.Delay(1000);
            }
        }

        private void ReceiveData(byte[] received)
        {
            
        }
    }
}