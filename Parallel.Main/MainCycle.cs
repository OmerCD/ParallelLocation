#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
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
using Terminal.Gui;

namespace Parallel.Main
{
    public partial class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IParseRouter<byte[]> _parseRouter;
        private readonly AppSettings _appSettings;
        private readonly IProcessManager _processManager;
        private readonly IListener _listener;
        private bool _isListening;

        private readonly IDictionary<int, IClient> _clients;

        public MainCycle(ILogger<MainCycle> logger,
            IParseRouter<byte[]> parseRouter, AppSettings appSettings, IProcessManager processManager)
        {
            _logger = logger;
            _parseRouter = parseRouter;
            _appSettings = appSettings;
            _processManager = processManager;
            _listener = new Listener(new byte[] {240, 240, 240, 240, 240}, new byte[] {241, 241, 241, 241, 241});
            _clients = new Dictionary<int, IClient>();
        }

        private void Close()
        {
            Terminal.Gui.Application.End(null);
        }

        private void StartListening()
        {
            _clients.Clear();
            if (_isListening)
            {
                _listener.ClientConnected -= ClientConnected;
                _listener.ClientDisconnected -= ClientDisconnected;
                _listener.StopListener();
                _labelListeningStatusValue.Text = "offline";
                _menuItemStartListening.Title = "_Start Listening";
                _listViewConnectedClients.SetSource(null);
                _listViewConnectedClients.SelectedItem = -1;
                _listViewConnectedClients.SelectedChanged-=ListViewConnectedClientsOnSelectedChanged;
                _labelConnectedClientCountValue.Text = "0";
                _isListening = false;
            }
            else
            {
                _listener.ClientConnected += ClientConnected;
                _listener.ClientDisconnected += ClientDisconnected;
                _listViewConnectedClients.SelectedChanged+=ListViewConnectedClientsOnSelectedChanged;
                _listener.StartReceive(
                    new BindInformation(_appSettings.ConnectionInfo.Port, _appSettings.ConnectionInfo.IpAddress),
                    ReceiveData);
                _labelListeningStatusValue.Text = "online";
                _menuItemStartListening.Title = "_Stop Listening";
                _isListening = true;
            }
        }

        private void ListViewConnectedClientsOnSelectedChanged()
        {
            var test = _listViewConnectedClients.SelectedItem;
        }

        private void ClientDisconnected(IClient obj)
        {
            List<string> source = _listener.ConnectedClients.Select(GetClientInfoText).ToList();
            _listViewConnectedClients.SetSource(source);
            _listViewConnectedClients.SetNeedsDisplay();
            _labelConnectedClientCountValue.Text = _listener.ClientCount.ToString();
        }

        private string GetClientInfoText(IClient client)
        {
            return
                $"{nameof(client.SocketId)}:{client.SocketId}";
        }

        private void ClientConnected(IClient client)
        {
            _listViewConnectedClients.SetSource(_listener.ConnectedClients.Select(GetClientInfoText).ToList());
            _labelConnectedClientCountValue.Text = _listener.ClientCount.ToString();
            _listViewConnectedClients.SetNeedsDisplay();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                InitializeGUI();
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
        }
    }
}