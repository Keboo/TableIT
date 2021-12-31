using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using TableIT.Remote.ViewModels;
using TableIT.Remote.Views;

namespace TableIT.Remote
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddSingleton<ControlPage>();
            builder.Services.AddSingleton<ControlPageViewModel>();
            
            builder.Services.AddSingleton<ImagesPage>();
            builder.Services.AddSingleton<ImagesPageViewModel>();

            builder.Services.AddSingleton<TableClientManager>();

            return builder.Build();
        }
    }
}