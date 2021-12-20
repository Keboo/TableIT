using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TableIT.Core
{
    public class TableClient : IAsyncDisposable
    {
        private readonly HubConnection _connection;

        public TableClient(string endpoint)
        {
            var url = endpoint;
            _connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public void Register<TMessage>(Action<TMessage> handler)
        {
            _connection.On<string>(typeof(TMessage).Name.ToLowerInvariant(), data =>
            {
                handler(System.Text.Json.JsonSerializer.Deserialize<TMessage>(data));
            });
        }

        public async Task SendAsync<TMessage>(TMessage message)
        {
            await _connection.SendAsync(typeof(TMessage).Name.ToLowerInvariant(), System.Text.Json.JsonSerializer.Serialize(message));
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}
