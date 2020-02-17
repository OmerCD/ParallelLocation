#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Parallel.Application.Services.Interfaces;
using Parallel.Application.ValueObjects;
using SocketListener;

namespace Parallel.Main
{
    [Obsolete]
    public partial class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IParseRouter<byte[]> _parseRouter;
        private readonly AppSettings _appSettings;
        private readonly IProcessManager _processManager;
        private readonly DynamicQueueListener _dynamicQueueListener;
        private readonly IListener _listener;
        private bool _isListening;
        private bool _isGuiActive;

        private readonly IDictionary<int, IClient> _clients;
        private int _clientIndexTracker = 0;

        public MainCycle(ILogger<MainCycle> logger,
            IParseRouter<byte[]> parseRouter, AppSettings appSettings, IProcessManager processManager, DynamicQueueListener dynamicQueueListener)
        {
            _logger = logger;
            _parseRouter = parseRouter;
            _appSettings = appSettings;
            _processManager = processManager;
            _dynamicQueueListener = dynamicQueueListener;
            _listener = new SocketListener.Listener(new byte[] {240, 240, 240, 240, 240}, new byte[] {241, 241, 241, 241, 241});
            _clients = new ConcurrentDictionary<int, IClient>();
        }

        private void Close()
        {
            Terminal.Gui.Application.End(null);
        }

        public void StartListening()
        {
            _clients.Clear();
            if (_isListening)
            {
              
                _listener.StopListener();
                if (_isGuiActive)
                {
                    _listener.ClientConnected -= ClientConnected;
                    _listener.ClientDisconnected -= ClientDisconnected;
                    _labelListeningStatusValue.Text = "offline";
                    _menuItemStartListening.Title = "_Start Listening";
                    _listViewConnectedClients.ClearClients();
                    _listViewConnectedClients.ClientSelected -= ListViewConnectedClientsOnSelectedChanged;
                    _labelConnectedClientCountValue.Text = "0";
                }
                
                _isListening = false;
            }
            else
            {
         
                if (_isGuiActive)
                {
                    _listener.ClientConnected += ClientConnected;
                    _listener.ClientDisconnected += ClientDisconnected;
                    _listViewConnectedClients.ClientSelected += ListViewConnectedClientsOnSelectedChanged;
                    _labelListeningStatusValue.Text = "online";
                    _menuItemStartListening.Title = "_Stop Listening";
                }
                _listener.StartReceive(
                    new BindInformation(_appSettings.ListeningPorts[0].Port, _appSettings.ConnectionInfo.IpAddress),
                    ReceiveData);
   
                _isListening = true;
            }
        }

        private void ListViewConnectedClientsOnSelectedChanged(IClient client)
        {
            _connectionDetailView.SetDetails(client);
        }

        private void ClientDisconnected(IClient obj)
        {
            _listViewConnectedClients.RemoveClient(obj.SocketId);
            _listViewConnectedClients.SetNeedsDisplay();
            _labelConnectedClientCountValue.Text = _listener.ClientCount.ToString();
            _clients.Remove(_clients.FirstOrDefault(x => x.Value.SocketId == obj.SocketId));
        }

        private string GetClientInfoText(IClient client)
        {
            return
                $"{client.SocketId}";
        }

        private void ClientConnected(IClient client)
        {
            _listViewConnectedClients.AddClient(client);
            _labelConnectedClientCountValue.Text = _listener.ClientCount.ToString();
            _listViewConnectedClients.SetNeedsDisplay();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                InitializeGUI();
                _isGuiActive = true;
            }
            catch (Exception ex)
            {
            }

            // Fancy();
            return Task.FromResult(0);
        }


        private async void Fancy()
        {
            while (true)
            {
                Console.Write("\rClient Count :" + _listener.ClientCount + "            ");
                await Task.Delay(1000);
            }
        }

        private void ReceiveData(byte[] received)
        {
            var message = _parseRouter.GetObject(received);
            if (message != null)
            {
                _processManager.Handle(message);
            }
        }
    }
}