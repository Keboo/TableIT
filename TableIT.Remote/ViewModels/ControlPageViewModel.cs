using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TableIT.Core;

namespace TableIT.Remote.ViewModels
{
    public class ControlPageViewModel : ObservableObject
    {
        private RemoteHandler RemoteHandler { get; }
        private TableHandler Client { get; }
        public IRelayCommand<PanDirection> PanCommand { get; }
        public IRelayCommand<string> ZoomCommand { get; }

        public ControlPageViewModel()
        {
            PanCommand = new RelayCommand<PanDirection>(OnPan);
            ZoomCommand = new RelayCommand<string>(OnZoom);

            RemoteHandler = new RemoteHandler(
                "https://tableitfunctions.azurewebsites.net/api",
                "test-user");

            Client = new TableHandler("https://tableitfunctions.azurewebsites.net/api",
                        "test-user");
            Task.Run(async () =>
            {
                try
                {
                    await Client.StartAsync();
                }
                catch (Exception e)
                { }
            });
        }

        private async void OnZoom(string zoomAdjustment)
        {
            await Client.Test();
            await Client.SendZoom(float.Parse(zoomAdjustment));
        }

        private async void OnPan(PanDirection direction)
        {
            switch(direction)
            {
                case PanDirection.Left:
                    await RemoteHandler.SendPan(-20, null);
                    break;
                case PanDirection.Right:
                    await RemoteHandler.SendPan(20, null);
                    break;
                case PanDirection.Up:
                    await RemoteHandler.SendPan(null, -20);
                    break;
                case PanDirection.Down:
                    await RemoteHandler.SendPan(null, 20);
                    break;
            }
        }
    }

    public enum PanDirection
    {
        Left,
        Up,
        Down,
        Right
    }
}
