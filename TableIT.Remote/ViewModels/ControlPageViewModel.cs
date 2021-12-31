using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TableIT.Core;

namespace TableIT.Remote.ViewModels
{
    public class ControlPageViewModel : ObservableObject
    {
        private TableClient? Client { get; set; }
        public IRelayCommand<PanDirection> PanCommand { get; }
        public IRelayCommand<string> ZoomCommand { get; }

        public IRelayCommand ConnectCommand { get; }

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
            PanCommand = new AsyncRelayCommand<PanDirection>(OnPan);
            ZoomCommand = new AsyncRelayCommand<string>(OnZoom);
            ConnectCommand = new AsyncRelayCommand(OnConnect);
        }

        private async Task OnZoom(string? zoomAdjustment)
        {
            if (string.IsNullOrEmpty(zoomAdjustment)) return;
            if (Client is { } client)
            {
                await client.SendZoom(float.Parse(zoomAdjustment));
            }
        }

        private async Task OnPan(PanDirection direction)
        {
            if (Client is { } client)
            {
                switch (direction)
                {
                    case PanDirection.Left:
                        await client.SendPan(-20, null);
                        break;
                    case PanDirection.Right:
                        await client.SendPan(20, null);
                        break;
                    case PanDirection.Up:
                        await client.SendPan(null, -20);
                        break;
                    case PanDirection.Down:
                        await client.SendPan(null, 20);
                        break;
                }
            }
        }

        private async Task OnConnect()
        {
            var targetUserId = TargetUserId;
            if (targetUserId?.Length != 6) return;
            await Task.Run(async () =>
            {
                try
                {
                    var client = Client = new TableClient(userId: targetUserId);
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
    }

    public enum PanDirection
    {
        Left,
        Up,
        Down,
        Right
    }
}
