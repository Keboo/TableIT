using Microsoft.Toolkit.Mvvm.Messaging;
using TableIT.Remote.Views;
using Application = Microsoft.Maui.Controls.Application;

namespace TableIT.Remote
{
    public partial class App : Application, IRecipient<TableClientConnectionStateChanged>
    {
        public App(AppShell appShell, ConnectPage connectPage, IMessenger messenger)
        {
            InitializeComponent();
            MainPage = connectPage;
            AppShell = appShell;
            messenger.Register(this);
        }

        public AppShell AppShell { get; }

        public void Receive(TableClientConnectionStateChanged message)
        {
            if (message.IsConnected &&
                MainPage is ConnectPage)
            {
                MainPage = AppShell;
            }
        }
    }
}
