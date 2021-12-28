using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TableIT.Core
{
    public class TableClient : IAsyncDisposable
    {
        private readonly HubConnection _connection;

        public string UserId { get; }

        public TableClient(string? endpoint = null, string? userId = null)
        {
            UserId = userId ?? GenerateUserId();
            _connection = new HubConnectionBuilder()
                .WithUrl(endpoint ?? "https://tableitfunctions.azurewebsites.net/api", options =>
                {
                    options.Headers["Authorization"] = ServiceUtils.GenerateAccessToken(UserId);
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

        private const string IdLetters = "ACDEFGHJKMNPRSTWXYZ12345679";
        private static Random Random { get; } = new Random();

        public static string GenerateUserId(int legnth = 6)
        {
            var letters = new char[legnth];
            for(int i =0; i < letters.Length; i++)
            {
                letters[i] = IdLetters[Random.Next(IdLetters.Length)];
            }
            return new string(letters);
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}
