using Terminal.Gui;

namespace Parallel.Main.Views
{
    public partial class ConnectionDetailView : Window
    {
        private Label _labelSocketIdText;
        private Label _labelSocketIdValue;

        private Label _labelRemoteIpText;
        private Label _labelRemoteIpValue;

        private void InitializeComponents()
        {
            _labelSocketIdText = new Label("Socket Id :")
            {
                X = 1,
                Y = 1
            };
            Add(_labelSocketIdText);
            _labelSocketIdValue = new Label("0")
            {
                X = Pos.Right(_labelSocketIdText),
                Y = Pos.Top(_labelSocketIdText)
            };
            Add(_labelSocketIdValue);
            _labelRemoteIpText = new Label("Remote Endpoint :")
            {
                X = Pos.Left(_labelSocketIdText),
                Y = Pos.Bottom(_labelSocketIdText)
            };
            Add(_labelRemoteIpText);
            _labelRemoteIpValue = new Label("0.0.0.0:0000")
            {
                X = Pos.Right(_labelRemoteIpText),
                Y = Pos.Top(_labelRemoteIpText)
            };
            Add(_labelRemoteIpValue);
        }
    }
}