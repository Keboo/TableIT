using System;
using System.Threading;
using System.Threading.Tasks;

namespace TableIT.Core.Tests;

public static class TableClientExtensions
{
    public static async Task<TMessage> WaitForMessage<TMessage>(this TableClient client, TimeSpan? timeout = null)
        where TMessage : class
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));
        TaskCompletionSource<TMessage> tcs = new();
        client.Register<TMessage>(msg =>
        {
            tcs.TrySetResult(msg);
        });
        cts.Token.Register(() => tcs.TrySetCanceled());
        
        return await tcs.Task;
    }
}

