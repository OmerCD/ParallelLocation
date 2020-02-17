using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parallel.Application.ValueObjects;

namespace Listener.WorkerService
{
    public class ListenerAdapter
    {
        public IEnumerable<IPortListener> GetTcpPortListeners(IEnumerable<ListeningPort> listeningPorts, string ipAddress)
        {
            return listeningPorts.Select(x => new TcpPortListener(x.StartBytes, x.EndBytes, x.Name, x.Port, ipAddress));
        }
    }
}