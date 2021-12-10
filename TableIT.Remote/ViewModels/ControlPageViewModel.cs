﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using TableIT.Core;

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
                "https://tableit.azurewebsites.net/message",
                "TestHub", "test-user");
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