using System;
using System.Collections.Generic;
using SocketListener;

namespace Listener.WorkerService
{
    /// <summary>
    /// Gelen paketleri dinler.
    /// </summary>
    public interface IPortListener
    {
        Action<IPortListener, byte[]> PackageCompleted { get; set; }
        Action<IClient> ClientConnected { get; set; }
        Action<IClient> ClientDisconnected { get; set; }

        /// <summary>
        /// For Queue Name
        /// </summary>
        string Name { get; }

        void StartListening();
        void StopListening();

        int ClientCount { get; }
        IEnumerable<IClient> ConnectedClients { get; }

        bool IsListening { get; set; }
    }
}