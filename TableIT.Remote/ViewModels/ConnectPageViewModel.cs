using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Remote.Messages;

namespace TableIT.Remote.ViewModels;

public class ConnectPageViewModel : ObservableObject
{
    public TableClientManager ClientManager { get; }
    public IMessenger Messenger { get; }
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

    public ConnectPageViewModel(TableClientManager clientManager, IMessenger messenger)
    {
        ConnectCommand = new AsyncRelayCommand(OnConnect);
        ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
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
            if (await client.PingTable())
            {
                Messenger.Send(new TableConnected());
            }
            else
            {
                ErrorMessage = $"Failed to find a table with id {userId}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error connecting" + Environment.NewLine + ex.Message;
        }
        IsConnecting = false;
    }
}
