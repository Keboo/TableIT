using Microsoft.Maui;
using Microsoft.Maui.Controls;
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
            Microsoft.Maui.Handlers.ScrollViewHandler.ScrollViewMapper.AppendToMapping(nameof(ScrollView.Orientation), (handler, view) =>
            {
                if (view.Orientation == ScrollOrientation.Both)
                {
#if ANDROID
                    handler.NativeView.HorizontalScrollBarEnabled = true;
                    handler.NativeView.VerticalScrollBarEnabled = true;
#endif
                }
            });

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
