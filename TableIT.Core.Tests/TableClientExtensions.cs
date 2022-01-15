using System;
using System.Threading;
using System.Threading.Tasks;

namespace TableIT.Core.Tests;

public static class TableClientExtensions
{
    public static async Task<TMessage> WaitForTableMessage<TMessage>(this TableClient client, TimeSpan? timeout = null)
        where TMessage : class
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));
        TaskCompletionSource<TMessage> tcs = new();
        client.RegisterTableMessage<TMessage>(msg =>
        {
            tcs.TrySetResult(msg);
        });
        cts.Token.Register(() => tcs.TrySetCanceled());
        
        return await tcs.Task;
    }
}

