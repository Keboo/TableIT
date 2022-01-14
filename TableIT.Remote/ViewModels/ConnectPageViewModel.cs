using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace TableIT.Remote.ViewModels
{
    public class ConnectPageViewModel : ObservableObject
    {
        public TableClientManager ClientManager { get; }
        public IRelayCommand ConnectCommand { get; }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }

        private string? _userId;
        public string? UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ConnectPageViewModel(TableClientManager clientManager)
        {
            ConnectCommand = new AsyncRelayCommand(OnConnect);
            ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            UserId = clientManager.UserId;
        }

        private async Task OnConnect()
        {
            ErrorMessage = null;
            string? userId = UserId = UserId?.Trim();
            if (userId?.Length != 6)
            {
                ErrorMessage = "Table ID must be exactly 6 characters";
                return;
            }
            IsConnecting = true;
            await Task.Yield();
            ClientManager.WithUserId(userId);
            var client = ClientManager.GetClient();
            try
            {
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error connecting" + Environment.NewLine + ex.Message;
            }
            IsConnecting = false;
        }
    }
}
