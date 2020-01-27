
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Parallel.Application.Hubs
{
    public class LocationHub:Hub
    {
        public void SendLocation(int tagId, double x, double z, double y)
        {
            Clients.All.SendAsync("LocationCreated", tagId, x, z, y);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("Disconnected. Exception :"+exception);
            return base.OnDisconnectedAsync(exception);
        }
    }
}