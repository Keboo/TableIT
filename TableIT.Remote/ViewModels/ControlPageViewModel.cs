using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using TableIT.Core;
using TableIT.Core.Messages;

namespace TableIT.Remote.ViewModels
{
    public class ControlPageViewModel : ObservableObject
    {
        private RemoteHandler RemoteHandler { get; }
        public IRelayCommand<PanDirection> PanCommand { get; }
        public IRelayCommand<string> ZoomCommand { get; }

        public ControlPageViewModel()
        {
            PanCommand = new RelayCommand<PanDirection>(OnPan);
            ZoomCommand = new RelayCommand<string>(OnZoom);

            RemoteHandler = new RemoteHandler(
                "https://tableit.service.signalr.net",
                "ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=",
                "TestHub");
        }

        private async void OnZoom(string zoomAdjustment)
        {
            await RemoteHandler.SendZoom(float.Parse(zoomAdjustment));
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
