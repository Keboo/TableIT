using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TableIT.Core;

namespace TableIT.Remote.ViewModels
{
    public class ControlPageViewModel : ObservableObject
    {
        private TableClient Client { get; }
        public IRelayCommand<PanDirection> PanCommand { get; }
        public IRelayCommand<string> ZoomCommand { get; }

        public ControlPageViewModel()
        {
            PanCommand = new RelayCommand<PanDirection>(OnPan);
            ZoomCommand = new RelayCommand<string>(OnZoom);

            Client = new TableClient("https://tableitfunctions.azurewebsites.net/api", "TABLE");
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
            await Client.SendZoom(float.Parse(zoomAdjustment));
        }

        private async void OnPan(PanDirection direction)
        {
            switch(direction)
            {
                case PanDirection.Left:
                    await Client.SendPan(-20, null);
                    break;
                case PanDirection.Right:
                    await Client.SendPan(20, null);
                    break;
                case PanDirection.Up:
                    await Client.SendPan(null, -20);
                    break;
                case PanDirection.Down:
                    await Client.SendPan(null, 20);
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
