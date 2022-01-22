using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using TableIT.Remote.Messages;
using TableIT.Remote.Views;

namespace TableIT.Remote
{
    public partial class AppShell : Shell, IRecipient<ImageSelected>
    {
        private IServiceProvider ServiceProvider { get; }

        public AppShell(
            IServiceProvider serviceProvider,
            IMessenger messenger)
        {
            InitializeComponent();
            ControlShell!.ContentTemplate = new DataTemplate(() => serviceProvider.GetRequiredService<ControlPage>());
            ImagesShell!.ContentTemplate = new DataTemplate(() => serviceProvider.GetRequiredService<ImagesPage>());
            //Disconnect!.ContentTemplate = new DataTemplate(() => serviceProvider.GetRequiredService<DisconnectPage>());
            messenger.Register(this);
            Routing.RegisterRoute("imagedetails", new SaneRouteFactory<ImageDetailsPage>(serviceProvider));
            ServiceProvider = serviceProvider;
        }

        private async void DisconnectClicked(object? sender, EventArgs e)
        {
            FlyoutIsPresented = false;
            await Navigation.PushModalAsync(ServiceProvider.GetRequiredService<DisconnectPage>());
        }

        async void IRecipient<ImageSelected>.Receive(ImageSelected message)
        {
            await GoToAsync($"imagedetails?imageId={message.ImageId}");
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            CurrentItem = Items[0];
            base.OnNavigatedFrom(args);
        }

        private class SaneRouteFactory<T> : RouteFactory
            where T : Element
        {
            private IServiceProvider ServiceProvider { get; }
            public SaneRouteFactory(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public override Element GetOrCreate() => ServiceProvider.GetRequiredService<T>(); 
        }
    }
}