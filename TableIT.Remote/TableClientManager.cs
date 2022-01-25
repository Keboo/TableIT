using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using TableIT.Core;

namespace TableIT.Remote
{
    public class TableClientManager
    {
        private const string LasUserIdKey = "TableIT.Connection.LastUserId";

        private IMessenger Messenger { get; }

        public TableClientManager(IMessenger messenger)
        {
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            UserId = Preferences.Get(LasUserIdKey, "");
        }

        public string? UserId { get; private set; }
        private TableClient? Client { get; set; }

        public TableClient GetClient()
        {
            var client = Client;
            if (client is null)
            {
                client = Client = new TableClient(userId: UserId);
                client.ConnectionStateChanged += ClientConnectionStateChanged;
            }
            return client;
        }

        private void ClientConnectionStateChanged(object? sender, EventArgs e)
        {
            bool isConnected = IsConnected;
            if (isConnected)
            {
                Preferences.Set(LasUserIdKey, Client!.UserId);
            }
            Messenger.Send(new TableClientConnectionStateChanged(isConnected));
        }

        internal void WithUserId(string userId)
        {
            if (userId != UserId)
            {
                UserId = userId;
                Client = null;
            }
        }

        public async Task Disconnect()
        {
            if (Client is { } client)
            {
                await client.DisposeAsync();
            }
            Client = null;
        }

        public bool IsConnected => Client?.ConnectionState == HubConnectionState.Connected;
    }

    public class TableClientConnectionStateChanged
    {
        public bool IsConnected { get; }

        public TableClientConnectionStateChanged(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
