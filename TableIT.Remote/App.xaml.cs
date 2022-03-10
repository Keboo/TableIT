using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using TableIT.Remote.Messages;
using TableIT.Remote.Views;
using Application = Microsoft.Maui.Controls.Application;

namespace TableIT.Remote
{
    public partial class App : Application,
        IRecipient<TableConnected>,
        IRecipient<TableDisconnected>
    {
        public App(Func<AppShell> appShell, Func<ConnectPage> connectPage, IMessenger messenger)
        {
            InitializeComponent();

//            Microsoft.Maui.Handlers.ScrollViewHandler.ScrollViewMapper.AppendToMapping(nameof(ScrollView.Orientation), (handler, view) =>
//            {
//                if (view.Orientation == ScrollOrientation.Both)
//                {
//#if ANDROID
//                    handler.NativeView.HorizontalScrollBarEnabled = true;
//                    handler.NativeView.VerticalScrollBarEnabled = true;
//#endif
//                }
//            });

            AppShell = appShell;
            ConnectPage = connectPage;
            MainPage = appShell();
            messenger.Register<TableConnected>(this);
            messenger.Register<TableDisconnected>(this);
        }

        private Func<AppShell> AppShell { get; }
        private Func<ConnectPage> ConnectPage { get; }

        protected override async void OnStart()
        {
            base.OnStart();
            if (MainPage is AppShell shell)
            {
                await shell.Navigation.PopToRootAsync(false);
                await shell.Navigation.PushModalAsync(ConnectPage());
            }
        }

        public async void Receive(TableConnected message)
        {
            if (MainPage is AppShell shell &&
                shell.Navigation.ModalStack.Count > 0 &&
                shell.Navigation.ModalStack[shell.Navigation.ModalStack.Count - 1] is ConnectPage)
            {
                await shell.Navigation.PopModalAsync();
            }
        }

        public async void Receive(TableDisconnected message)
        {
            if (MainPage is AppShell shell)
            {
                await shell.Navigation.PopToRootAsync(false);
                await shell.Navigation.PushModalAsync(ConnectPage());
                
            }
        }
    }
}
