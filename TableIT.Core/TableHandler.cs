using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TableIT.Core
{
    public class TableHandler : IAsyncDisposable
    {
        private readonly HubConnection _connection;

        public TableHandler(string endpoint, string userId)
        {
            //var url = $"{endpoint}?user={userId}";
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
            _connection.On<TMessage>(typeof(TMessage).Name.ToLowerInvariant(), data =>
            {
                //handler(System.Text.Json.JsonSerializer.Deserialize<TMessage>(data));
                handler(data);
            });
        }

        public async Task Test()
        {
            await _connection.SendAsync("SignalRTest", "test message");
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
