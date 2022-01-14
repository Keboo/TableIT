using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Hosting;
using Microsoft.Toolkit.Mvvm.Messaging;
using TableIT.Remote.ViewModels;
using TableIT.Remote.Views;

namespace TableIT.Remote
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureEssentials();

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddSingleton<ControlPage>();
            builder.Services.AddSingleton<ControlPageViewModel>();
            
            builder.Services.AddSingleton<ImagesPage>();
            builder.Services.AddSingleton<ImagesPageViewModel>();

            builder.Services.AddSingleton<ConnectPage>();
            builder.Services.AddSingleton<ConnectPageViewModel>();

            builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            builder.Services.AddSingleton<TableClientManager>();

            return builder.Build();
        }
    }
}