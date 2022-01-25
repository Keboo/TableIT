using System;
using System.Collections.Generic;
using System.Threading;

namespace TableIT.Core;

public sealed class CancellationManager : IDisposable
{
    private CancellationTokenSource? _tokenSource;
    private TimeSpan? _timeout;
    private bool _isDisposed;
    private readonly List<CancellationToken> _linkedTokens = new();

    public void Cancel()
    {
        var tokenSource = Interlocked.Exchange(ref _tokenSource, null);
        if (tokenSource != null)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
    }

    public CancellationToken GetNextToken(bool cancelPrevious = true)
    {
        var newTokenSource = new CancellationTokenSource();
        var tokenSource = Interlocked.Exchange(ref _tokenSource, newTokenSource);
        if (tokenSource != null)
        {
            if (cancelPrevious)
            {
                tokenSource.Cancel();
            }
            tokenSource.Dispose();
        }
        if (_timeout is { } timeout)
        {
            newTokenSource.CancelAfter(timeout);
        }
        return newTokenSource.Token;
    }

    public void CancelAfter(TimeSpan timeout)
    {
        _timeout = timeout;
        _tokenSource?.CancelAfter(timeout);
    }

    public void AddLinkedToken(CancellationToken token)
    {
        if (token.CanBeCanceled)
        {
            _linkedTokens.Add(token);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                var tokenSource = Interlocked.Exchange(ref _tokenSource, null);
                if (tokenSource != null)
                {
                    tokenSource.Dispose();
                }
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
