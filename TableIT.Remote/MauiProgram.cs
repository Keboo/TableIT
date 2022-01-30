using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Hosting;
using Microsoft.Toolkit.Mvvm.Messaging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System;
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
                .UseSkiaSharp()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureEssentials();

            builder.Services.AddTransient<AppShell>();
            AddFactory<AppShell>(builder.Services);

            builder.Services.AddSingleton<ControlPage>();
            builder.Services.AddSingleton<ControlPageViewModel>();
            
            builder.Services.AddSingleton<ImagesPage>();
            builder.Services.AddSingleton<ImagesPageViewModel>();

            AddFactory<ConnectPage>(builder.Services);
            builder.Services.AddSingleton<ConnectPage>();
            builder.Services.AddSingleton<ConnectPageViewModel>();

            builder.Services.AddSingleton<ImageDetailsPage>();
            builder.Services.AddSingleton<ImageDetailsPageViewModel>();

            builder.Services.AddSingleton<DisconnectPage>();
            builder.Services.AddSingleton<DisconnectPageViewModel>();

            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<SettingsPageViewModel>();


            builder.Services.AddSingleton<Imaging.IImageManager, Imaging.ImageManager>();
            builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            builder.Services.AddSingleton<TableClientManager>();

            return builder.Build();

            static void AddFactory<T>(IServiceCollection services)
                where T : class
                => services.AddSingleton<Func<T>>(sp => () => sp.GetRequiredService<T>());
        }
    }
}