using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core
{
    public class TableClient : IAsyncDisposable
    {
        private class MessageCache
        {
            private class CacheItem
            {
                public DateTime Expiration { get; } = DateTime.Now + TimeSpan.FromMinutes(15);

                private List<(int, string)> Items { get; } = new();

                public int Add(int index, string data)
                {
                    lock(Items)
                    {
                        Items.Add((index, data));
                        return Items.Count;
                    }
                }
            }

            private Dictionary<Guid, CacheItem> Items { get; } = new();

            public int AddData(Guid id, int index, string data)
            {
                CacheItem item;
                lock(Items)
                {
                    if (!Items.TryGetValue(id, out item))
                    {
                        Items[id] = item = new CacheItem();
                    }
                }
                return item.Add(index, data);
            }

            public string? GetMessage(Guid id)
            {
                lock(Items)
                {
                    if (Items.TryGetValue(id, out CacheItem item))
                    {
                        return item.GetData();
                    }
                }
                return null;
            }
        }

        public event EventHandler ConnectionStateChanged;
        private readonly HubConnection _connection;
        private MessageCache Cache { get; } = new();

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

            _connection.Closed += ConnectionClosed;
            _connection.Reconnected += ConnectionReconnected;
            _connection.Reconnecting += ConnectionReconnecting;
        }

        private Task ConnectionReconnecting(Exception? arg)
        {
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private Task ConnectionReconnected(string? arg)
        {
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private Task ConnectionClosed(Exception? arg)
        {
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public HubConnectionState ConnectionState => _connection.State;

        public void Register<TMessage>(Action<TMessage> handler)
            where TMessage : class
        {
            //TODO handle return from On<>
            _connection.On<string>(typeof(TMessage).Name.ToLowerInvariant(), data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null &&
                    GetMessage<TMessage>(envelope) is { } message)
                {
                    handler(message);
                }
            });
        }

        private TMessage? GetMessage<TMessage>(EnvelopeMessage envelopeMessage)
            where TMessage : class
        {
            if (envelopeMessage.TotalParts == 1)
            {
                return JsonSerializer.Deserialize<TMessage>(envelopeMessage.Data ?? "");
            }
            int received = Cache.AddData(envelopeMessage.GroupId, envelopeMessage.Index, envelopeMessage.Data ?? "");
            if (received == envelopeMessage.TotalParts)
            {
                return Cache.GetMessage<TMessage>(envelopeMessage.GroupId);
            }
            return null;
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

        public async Task SendAsync<TMessage>(TMessage message, bool allowSplitting = false)
        {
            const int payloadSize = 5_000;
            string data = JsonSerializer.Serialize(message);

            int totalParts = data.Length / payloadSize;
            if (data.Length % payloadSize != 0) totalParts++;
            Guid id = Guid.NewGuid();

            for (int index = 0; index < totalParts; index++)
            {
                var envelope = new EnvelopeMessage
                {
                    GroupId = id,
                    Index = index,
                    TotalParts = totalParts,
                    Data = data[(index * payloadSize)..payloadSize]
                };

                await _connection.SendAsync(typeof(TMessage).Name.ToLowerInvariant(), JsonSerializer.Serialize(envelope));
            }
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private const string IdLetters = "ACDEFGHJKMNPRSTWXYZ1234679";
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
