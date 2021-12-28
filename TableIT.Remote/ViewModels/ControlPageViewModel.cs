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

        private string? _status;
        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string? _targetUserId;
        public string? TargetUserId
        {
            get => _targetUserId;
            set => SetProperty(ref _targetUserId, value);
        }

        public ControlPageViewModel()
        {
            PanCommand = new RelayCommand<PanDirection>(OnPan);
            ZoomCommand = new RelayCommand<string>(OnZoom);

            Client = new TableClient();
            Task.Run(async () =>
            {
                try
                {
                    Status = "Connecting...";
                    await Client.StartAsync();
                    Status = "Connected";
                }
                catch (Exception e)
                {
                    Status = $"Error: {e.Message}";
                }
            });
        }

        private async void OnZoom(string? zoomAdjustment)
        {
            if (string.IsNullOrEmpty(zoomAdjustment)) return;
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
