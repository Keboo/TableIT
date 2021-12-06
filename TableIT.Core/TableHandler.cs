using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TableIT.Core
{
    public class TableHandler : IAsyncDisposable
    {
        private readonly HubConnection _connection;

        public TableHandler(string endpoint, string accessKey, string hubName, string userId)
        {
            var url = GetClientUrl(endpoint, hubName);

            _connection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(ServiceUtils.GenerateAccessToken(accessKey, url, userId));
                    };
                }).Build();
        }

        public void Register<TMessage>(Action<TMessage> handler)
        {
            _connection.On<string>(typeof(TMessage).Name.ToLowerInvariant(), data =>
            {
                handler(System.Text.Json.JsonSerializer.Deserialize<TMessage>(data));
            });
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        private string GetClientUrl(string endpoint, string hubName)
        {
            return $"{endpoint}/client/?hub={hubName}";
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}
