using Parallel.Main.Views;
using Terminal.Gui;

namespace Parallel.Main
{
    public partial class MainCycle
    {
        private Window _win;

        private MenuBar _menuBar;
        
        private MenuBarItem _menuBarItemConnections;
        private MenuItem _menuItemStartListening;
        
        private MenuBarItem _menuBarItemMain;
        private MenuItem _menuItemClose;

        private ListView _listViewConnectedClients;

        private ConnectionDetailView _connectionDetailView;
        public void InitializeTopMenuComponents(View parent)
        {
            _menuItemStartListening = new MenuItem("_Start Listening", "", StartListening);
            _menuBarItemConnections =
                new MenuBarItem("Connection", new[] {_menuItemStartListening});

            _menuItemClose = new MenuItem("Close","", null);
            _menuBarItemMain = new MenuBarItem("Main", new[] {_menuItemClose});

            _menuBar = new MenuBar(new []
            {
                _menuBarItemMain,
                _menuBarItemConnections
            });
            
            parent.Add(_menuBar);
        }
        
        private Label _labelListeningStatusText;
        private Label _labelListeningStatusValue;
        private Label _labelConnectedClientCountText;
        private Label _labelConnectedClientCountValue;
        
        public void InitializeWinComponents(View parent)
        {
            _labelListeningStatusText = new Label(3,2, "Status :");
            parent.Add(_labelListeningStatusText);

            _labelListeningStatusValue = new Label("offline")
            {
                X = Pos.Right(_labelListeningStatusText),
                Y = Pos.Top(_labelListeningStatusText)
            };
            parent.Add(_labelListeningStatusValue);
            
            _listViewConnectedClients = new ListView(new []{"SocketList"})
            {
                X = Pos.Left(_labelListeningStatusText),
                Y = Pos.Bottom(_labelListeningStatusText),
                Width = Dim.Percent(40),
                Height = Dim.Percent(90),
                AllowsMarking = true
            };
            parent.Add(_listViewConnectedClients);

            _labelConnectedClientCountText = new Label("Client Count :")
            {
                X = Pos.Left(_listViewConnectedClients),
                Y = Pos.Bottom(_listViewConnectedClients)
            };
            parent.Add(_labelConnectedClientCountText);

            _labelConnectedClientCountValue = new Label("0")
            {
                X = Pos.Right(_labelConnectedClientCountText),
                Y = Pos.Top(_labelConnectedClientCountText)
            };
            parent.Add(_labelConnectedClientCountValue);
            
            _connectionDetailView = new ConnectionDetailView()
            {
                X= Pos.Right(_listViewConnectedClients),
                Y= 1,
                Height = Dim.Percent(30),
                Width = Dim.Percent(90)
            };
            parent.Add(_connectionDetailView);
        }
        public void InitializeGUI()
        {
            Terminal.Gui.Application.Init();
            var top = Terminal.Gui.Application.Top;

            _win = new Window("Parallel")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            InitializeWinComponents(_win);
            top.Add(_win);

            InitializeTopMenuComponents(top);
            Terminal.Gui.Application.Run();
        }
    }
}