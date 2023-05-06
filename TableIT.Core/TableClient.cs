using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;
using TableIT.Core.Resources;

namespace TableIT.Core;

public partial class TableClient : IAsyncDisposable
{
    public event EventHandler? ConnectionStateChanged;
    private readonly HubConnection _connection;

    private ResourceManager ResourceManager { get; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    public string Endpoint { get; }

    public string UserId { get; }

    public TableClient(string? endpoint = null, string? userId = null)
    {
        Timeout = TimeSpan.FromSeconds(5);
#if DEBUG
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Timeout = TimeSpan.FromMinutes(10);
        }
#endif
        UserId = userId ?? GenerateUserId();
        Endpoint = endpoint ??= "https://tableitfunctions.azurewebsites.net";
        ResourceManager = new ResourceManager(endpoint);

        _connection = new HubConnectionBuilder()
            .WithUrl(endpoint + "/api", options =>
            {
                options.Headers["Authorization"] = ServiceUtils.GenerateAccessToken(UserId);
            })
            .WithAutomaticReconnect()
            .Build();

        _ = _connection.On<string>("tablemessage", async data =>
        {
            await Task.Run(async () =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });
        });
        _ = _connection.On<string>(typeof(ResponseMessage).Name.ToLowerInvariant(), async data =>
        {
            await Task.Run(async () =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });
        });
        _ = _connection.On<string>(typeof(RequestMessage).Name.ToLowerInvariant(), async data =>
        {
            await Task.Run(async () =>
            {
                var envelope = JsonSerializer.Deserialize<EnvelopeMessage>(data);
                if (envelope is not null)
                {
                    await ProcessMessage(envelope);
                }
            });
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

    public IDisposable Register<TMessage>(Action<TMessage> handler)
        where TMessage : class
        => Register(handler, null);

    private IDisposable Register<T>(Action<T> handler, Guid? forGroupId)
        where T : class
        => Register(new Func<T, Task<EnvelopeResponse?>>(async msg =>
        {
            await Task.Yield();
            handler(msg);
            return null;
        }), forGroupId);

    private IDisposable Register<T>(Func<T, Task<EnvelopeResponse?>> handler, Guid? forGroupId)
        where T : class
    {
        Func<EnvelopeMessage, Task<EnvelopeResponse?>> envelopeHandler = new(async msg =>
        {
            if (forGroupId != null && msg.GroupId != forGroupId) return null;
            return await Task.Run(async () =>
            {
                if (GetMessage<T>(msg) is { } response)
                {
                    return await handler(response);
                }
                return null;
            });
        });
        List<Func<EnvelopeMessage, Task<EnvelopeResponse?>>>? handlers;
        lock (Handlers)
        {
            if (!Handlers.TryGetValue(typeof(T).FullName!, out handlers))
            {
                Handlers[typeof(T).FullName!] = handlers = new();
            }
            handlers.Add(envelopeHandler);
        }
        return new RemoveFromList<Func<EnvelopeMessage, Task<EnvelopeResponse?>>>(handlers, envelopeHandler);

        static TMessage? GetMessage<TMessage>(EnvelopeMessage envelopeMessage)
            where TMessage : class
        {
            return JsonSerializer.Deserialize<TMessage>(envelopeMessage.Data ?? "");
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

    public async Task<IReadOnlyList<ImageData>> GetImages()
        => await ResourceManager.GetImages();

    public async Task<Stream?> GetImage(
        string imageId,
        string? version,
        int? width = null,
        int? height = null) => await ResourceManager.Get(imageId, version, width, height);

    public async Task<ImageData?> ImportImage(
        string name,
        Stream imageStream,
        IProgress<double>? progress = null)
        => await ResourceManager.Import(imageStream, name);

    public async Task<bool> DeleteImage(
        string name,
        string? version)
        => await ResourceManager.Delete(name, version);

    private async Task<bool> SendAsync(
        string methodName,
        Type dataType,
        string data,
        Guid? groupId = null,
        IProgress<double>? progress = null)
    {
        groupId ??= Guid.NewGuid();

        var envelope = new EnvelopeMessage
        {
            GroupId = groupId.Value,
            DataType = dataType.FullName,
            Data = data
        };
        await _connection.SendAsync(methodName, JsonSerializer.Serialize(envelope));
        return true;
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

    public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request, TimeSpan? cancelAfter = null)
        where TResponse : class
    {
        Guid groupId = Guid.NewGuid();

        var tcs = new TaskCompletionSource<TResponse?>();

        using Timer timeout = new(_ =>
        {
            Debug.WriteLine($"{DateTime.Now.TimeOfDay} timeout {request}");
            tcs.TrySetCanceled();
        });
        long messageTimeout = (long)(cancelAfter ?? Timeout).TotalMilliseconds;
        using IDisposable unregister = Register<TResponse>(response =>
        {
            Debug.WriteLine($"{DateTime.Now.TimeOfDay} response done {request}");
            tcs.TrySetResult(response);
        },
        groupId);

        timeout.Change(messageTimeout, System.Threading.Timeout.Infinite);
        try
        {
            if (!await SendAsync(typeof(RequestMessage).Name.ToLowerInvariant(), typeof(TRequest), JsonSerializer.Serialize(request), groupId))
            {
                Debug.WriteLine($"{DateTime.Now.TimeOfDay} failed to send {request}");
                tcs.TrySetCanceled();
            }
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"{DateTime.Now.TimeOfDay} cancelled {request}");
            return null;
        }
    }

    public async Task StartAsync()
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            await _connection.StartAsync();
        }
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

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    private class RemoveFromList<T> : IDisposable
    {
        public RemoveFromList(IList<T> list, T item)
        {
            List = list;
            Item = item;
        }

        private IList<T> List { get; }
        private T Item { get; }

        public void Dispose()
        {
            if (!List.Remove(Item))
            {

            }
        }
    }
}
