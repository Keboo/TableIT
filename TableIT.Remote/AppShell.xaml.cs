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
        public AppShell(
            ControlPage controlPage,
            ImagesPage imagesPage,
            IServiceProvider serviceProvider,
            IMessenger messenger)
        {
            InitializeComponent();
            ControlShell!.ContentTemplate = new DataTemplate(() => controlPage);
            ImagesShell!.ContentTemplate = new DataTemplate(() => imagesPage);
            messenger.Register(this);

            Routing.RegisterRoute("imagedetails", new SaneRouteFactory<ImageDetailsPage>(serviceProvider));
        }

        async void IRecipient<ImageSelected>.Receive(ImageSelected message)
        {
            await GoToAsync($"imagedetails?imageId={message.ImageId}");
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