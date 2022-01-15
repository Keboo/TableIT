using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    lock (Items)
                    {
                        Items.Add((index, data));
                        return Items.Count;
                    }
                }

                public string GetData()
                {
                    StringBuilder sb = new();
                    foreach (var item in Items.OrderBy(t => t.Item1))
                    {
                        sb.Append(item.Item2);
                    }
                    return sb.ToString();
                }
            }

            private Dictionary<Guid, CacheItem> Items { get; } = new();

            public int AddData(Guid id, int index, string data)
            {
                CacheItem item;
                lock (Items)
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
                lock (Items)
                {
                    if (Items.TryGetValue(id, out CacheItem item))
                    {
                        return item.GetData();
                    }
                }
                return null;
            }

            public T? GetMessage<T>(Guid id) where T : class
            {
                if (GetMessage(id) is { } stringData)
                {
                    return JsonSerializer.Deserialize<T>(stringData);
                }
                return null;
            }
        }

        public event EventHandler? ConnectionStateChanged;
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

        public IDisposable Register<TMessage>(Action<TMessage> handler)
            where TMessage : class
            => Register(typeof(TMessage).Name.ToLowerInvariant(), handler);

        public IDisposable Register<TMessage>(string methodName, Action<TMessage> handler)
            where TMessage : class
        {
            return _connection.On<string>(methodName, data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null &&
                    envelope.DataType == typeof(TMessage).FullName &&
                    GetMessage<TMessage>(envelope) is { } message)
                {
                    handler(message);
                }
            });
        }

        public async Task SendAsync<TMessage>(string methodName, TMessage message)
        {
            const int payloadSize = 5_000;
            string data = JsonSerializer.Serialize(message);

            int totalParts = data.Length / payloadSize;
            if (data.Length % payloadSize != 0) totalParts++;
            Guid id = Guid.NewGuid();

            for (int index = 0; index < totalParts; index++)
            {
                int dataIndex = index * payloadSize;
                var dataSize = Math.Min(payloadSize, data.Length - dataIndex);
                var envelope = new EnvelopeMessage
                {
                    GroupId = id,
                    Index = index,
                    TotalParts = totalParts,
                    DataType = typeof(TMessage).FullName,
                    Data = data.Substring(dataIndex, dataSize)
                };

                await _connection.SendAsync(methodName, JsonSerializer.Serialize(envelope));
            }
        }

        public Task SendAsync<TMessage>(TMessage message)
            => SendAsync(typeof(TMessage).Name.ToLowerInvariant(), message);

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

        public void Handle<TRequest, TResponse>(Func<TRequest, Task<TResponse?>> asyncHandler)
        {
            _connection.On<string>(typeof(RequestMessage).Name.ToLowerInvariant(), async data =>
            {
                var request = JsonSerializer.Deserialize<RequestMessage>(data);
                if (typeof(TRequest).FullName == request?.RequestType)
                {
                    TRequest? requestData = JsonSerializer.Deserialize<TRequest>(request.RequestData);
                    TResponse? responseData = requestData is not null ? await asyncHandler(requestData) : default;
                    ResponseMessage response = new()
                    {
                        RequestId = request.RequestId,
                        ResponseType = typeof(TResponse).FullName
                    };
                    if (responseData is not null)
                    {
                        response.ResponseData = JsonSerializer.Serialize(responseData);
                    }
                    await SendAsync(response);
                }
            });
        }

        public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken? token = null)
        {
            RequestMessage requestMessage = new()
            {
                RequestId = Guid.NewGuid(),
                RequestData = JsonSerializer.Serialize(request),
                RequestType = typeof(TRequest).FullName!
            };

            //TODO: reduce allocations when not needed
            using CancellationTokenSource cts = new();
            if (token is null)
            {
                //TODO: make default configurable
                TimeSpan timeout = TimeSpan.FromSeconds(5);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    timeout = TimeSpan.FromMinutes(30);
                }
#endif
                cts.CancelAfter(timeout);
                token = cts.Token;
            }

            TResponse? response = default;
            using SemaphoreSlim waitHandle = new(0);
            using IDisposable _ = _connection.On<string>(typeof(ResponseMessage).Name.ToLowerInvariant(), data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null &&
                    GetMessage<ResponseMessage>(envelope) is { } message &&
                    message.RequestId == requestMessage.RequestId && 
                    message.ResponseType == typeof(TResponse).FullName)
                {
                    response = JsonSerializer.Deserialize<TResponse>(message.ResponseData);
                    waitHandle.Release();
                }
            });

            await _connection.SendAsync(typeof(RequestMessage).Name.ToLowerInvariant(), JsonSerializer.Serialize(requestMessage));
            await waitHandle.WaitAsync(token ?? CancellationToken.None);

            return response;
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
