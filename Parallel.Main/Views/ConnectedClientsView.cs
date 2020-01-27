using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SocketListener;
using Terminal.Gui;

namespace Parallel.Main.Views
{
    public partial class ConnectedClientsView
    {
        private readonly IDictionary<Guid, ClientButton> _clientList = new ConcurrentDictionary<Guid, ClientButton>();
        private int _heightTracker = 0;
        public event Action<IClient> ClientSelected;
        public void AddClient(IClient client)
        {
            lock (_clientList)
            {
                var button = new Button(1, _heightTracker, client.SocketId.ToString());
                Interlocked.Increment(ref _heightTracker);
                _clientList.Add(client.SocketId, new ClientButton(button, client));
                button.Clicked += () => ClientSelected?.Invoke(client);
                Add(button);
            }
        }

        public void ClearClients()
        {
            RemoveAll();
            _heightTracker = 0;
            _clientList.Clear();
        }

        public void SetClients(IEnumerable<IClient> clients)
        {
            _heightTracker = 0;
            Clear();
            foreach (IClient client in clients)
            {
                AddClient(client);
            }
        }

        public void RemoveClient(Guid id)
        {
            if (_clientList.ContainsKey(id))
            {
                _clientList.Remove(id);
                SetClients(_clientList.Values.Select(x => x.Client));
            }
        }

        private struct ClientButton
        {
            public ClientButton(Button button, IClient client)
            {
                Button = button;
                Client = client;
            }

            public Button Button { get; set; }
            public IClient Client { get; set; }
        }
    }
}