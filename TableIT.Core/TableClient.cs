using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core
{

    public partial class TableClient : IAsyncDisposable
    {
        public event EventHandler? ConnectionStateChanged;
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

            _ = _connection.On<string>("tablemessage", async data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });
            _ = _connection.On<string>(typeof(ResponseMessage).Name.ToLowerInvariant(), async data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });
            _ = _connection.On<string>(typeof(RequestMessage).Name.ToLowerInvariant(), async data =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });


            _connection.Closed += ConnectionClosed;
            _connection.Reconnected += ConnectionReconnected;
            _connection.Reconnecting += ConnectionReconnecting;
        }

        private Dictionary<string, List<Func<EnvelopeMessage, Task<EnvelopeResponse?>>>> Handlers { get; } = new();

        private async Task ProcessMessage(EnvelopeMessage envelope)
        {
            List<Func<EnvelopeMessage, Task<EnvelopeResponse?>>>? handlers;
            lock (Handlers)
            {
                if (!Handlers.TryGetValue(envelope.DataType ?? "", out handlers))
                {
                    handlers = new(handlers);
                    return;
                }
            }
            foreach (var handler in handlers)
            {
                if (await handler(envelope) is { } response &&
                    response.MethodName is { } methodName &&
                    response.DataType is { } dataType &&
                    response.Data is { } data)
                {
                    await SendAsync(methodName, dataType, data, envelope.GroupId);
                }
            }
        }

        public void Register<TMessage>(Action<TMessage> handler)
            where TMessage : class
            => Register<TMessage>(handler, null);

        private void Register<T>(Action<T> handler, Guid? forGroupId)
            where T : class
            => Register(new Func<T, Task<EnvelopeResponse?>>(msg =>
            {
                handler(msg);
                return Task.FromResult<EnvelopeResponse?>(null);
            }), forGroupId);

        private void Register<T>(Func<T, Task<EnvelopeResponse?>> handler, Guid? forGroupId)
            where T : class
        {
            lock (Handlers)
            {
                if (!Handlers.TryGetValue(typeof(T).FullName, out List<Func<EnvelopeMessage, Task<EnvelopeResponse?>>> handlers))
                {
                    Handlers[typeof(T).FullName] = handlers = new();
                }
                handlers.Add(async msg =>
                {
                    if (forGroupId != null && msg.GroupId != forGroupId) return null;
                    if (GetMessage<T>(msg) is { } response)
                    {
                        return await handler(response);
                    }
                    return null;
                });
            }
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

        public async Task SendAsync<TMessage>(string methodName, TMessage message)
        {
            string data = JsonSerializer.Serialize(message);
            await SendAsync(methodName, typeof(TMessage), data);
        }

        private async Task SendAsync(string methodName, Type dataType, string data, Guid? groupId = null)
        {
            const int payloadSize = 5_000;
            groupId ??= Guid.NewGuid();

            int totalParts = data.Length / payloadSize;
            if (data.Length % payloadSize != 0) totalParts++;

            for (int index = 0; index < totalParts; index++)
            {
                int dataIndex = index * payloadSize;
                var dataSize = Math.Min(payloadSize, data.Length - dataIndex);
                var envelope = new EnvelopeMessage
                {
                    GroupId = groupId.Value,
                    Index = index,
                    TotalParts = totalParts,
                    DataType = dataType.FullName,
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
            where TRequest : class
        {
            Register<TRequest>(async request =>
            {
                if (await asyncHandler(request) is { } responseData)
                {
                    return new EnvelopeResponse()
                    {
                        MethodName = typeof(ResponseMessage).Name.ToLowerInvariant(),
                        DataType = typeof(TResponse),
                        Data = JsonSerializer.Serialize(responseData)
                    };
                }
                return null;
            }, null);
        }

        public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken? token = null)
            where TResponse : class
        {
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

            Guid groupId = Guid.NewGuid();

            var tcs = new TaskCompletionSource<TResponse?>();
            token.Value.Register(() => tcs.TrySetResult(null));
            
            Register<TResponse>(response =>
            {
                tcs.TrySetResult(response);
            }, groupId);

            await SendAsync(typeof(RequestMessage).Name.ToLowerInvariant(), typeof(TRequest), JsonSerializer.Serialize(request), groupId);
            return await tcs.Task;
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
