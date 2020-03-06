using System;
using SocketListener;
using Terminal.Gui;

namespace Parallel.Main.Views
{
    [Obsolete]
    public partial class ConnectionDetailView
    {
        public ConnectionDetailView() : base("Connection Detail")
        {
            InitializeComponents();
        }

        public void SetDetails(IClient client)
        {
            _labelRemoteIpValue.Text = client.Socket.RemoteEndPoint.ToString();
            _labelSocketIdValue.Text = client.SocketId.ToString();
        }
    }
}