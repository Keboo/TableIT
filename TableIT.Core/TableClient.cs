using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;

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
            //TODO handle return from On<>
            _connection.On<string>(typeof(TMessage).Name.ToLowerInvariant(), data =>
            {
                handler(JsonSerializer.Deserialize<TMessage>(data));
            });
        }

        public void Handle<TRequest, TResponse>(Func<TRequest, Task<TResponse>> asyncHandler)
        {
            _connection.On<string>(typeof(RequestMessage).Name.ToLowerInvariant(), async data =>
            {
                var request = JsonSerializer.Deserialize<RequestMessage>(data);
                if (typeof(TRequest).FullName == request?.RequestType)
                {
                    TRequest? requestData = JsonSerializer.Deserialize<TRequest>(request.RequestData);
                    TResponse responseData = await asyncHandler(requestData);
                    if (responseData is not null)
                    {
                        ResponseMessage response = new()
                        {
                            RequestId = request.RequestId,
                            ResponseData = JsonSerializer.Serialize(responseData),
                            ResponseType = typeof(TResponse).FullName
                        };
                        await SendAsync(response);
                    }
                }
            });
        }

        public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request)
        {
            RequestMessage requestMessage = new()
            {
                RequestId = Guid.NewGuid(),
                RequestData = JsonSerializer.Serialize(request),
                RequestType = typeof(TRequest).FullName!
            };

            TResponse? response = default;
            using SemaphoreSlim waitHandle = new(0);
            //TODO: Timeout/Cancellation
            using IDisposable _ = _connection.On<string>(typeof(ResponseMessage).Name.ToLowerInvariant(), data =>
            {
                var responseMessage = JsonSerializer.Deserialize<ResponseMessage>(data);
                if (responseMessage?.RequestId == requestMessage.RequestId)
                {
                    if (responseMessage.ResponseType == typeof(TResponse).FullName)
                    {
                        response = JsonSerializer.Deserialize<TResponse>(responseMessage.ResponseData);
                    }

                    waitHandle.Release();
                }
            });


            await _connection.SendAsync(typeof(RequestMessage).Name.ToLowerInvariant(), JsonSerializer.Serialize(requestMessage));
            await waitHandle.WaitAsync();

            return response;
        }

        public async Task SendAsync<TMessage>(TMessage message)
        {
            await _connection.SendAsync(typeof(TMessage).Name.ToLowerInvariant(), JsonSerializer.Serialize(message));
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
            for (int i = 0; i < letters.Length; i++)
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
