using System.Collections.Concurrent;
using TableIT.Shared;

namespace TableIT.Server.Hubs;

public interface ITableManager
{
    Task<TableState> AddViewerConnectionToTableAsync(string tableId, string connectionId);
    Task<TableState> AddRemoteConnectionToTableAsync(string tableId, string connectionId);
    Task<TableState> AddTableConnectionAsync(string tableId, TableConfiguration tableConfiguration, string connectionId);

    Task<TableState?> GetTableStateAsync(string connectionId);
    Task<TableState?> UpdateTableStateAsync(string connectionId, Func<TableState, TableState> updateTableState);
}

public class InMemoryTableManager : ITableManager
{
    private ConcurrentDictionary<string, SemaphoreSlim> TableLocks { get; } = new();
    private ConcurrentDictionary<string, TableState> Tables { get; } = new();
    private ConcurrentDictionary<string, string> ConnectionsToTable { get; } = new();

    public Task<TableState> AddViewerConnectionToTableAsync(string tableId, string connectionId)
    {
        ConnectionsToTable.AddOrUpdate(connectionId, tableId , (_, _) => tableId);

        return WithLock(tableId, () =>
        {
            TableState rv = Tables.AddOrUpdate(tableId, new TableState(tableId, null), (_, existing) =>
            {
                return existing with
                {
                    ViewerConnections = existing.ViewerConnections.Append(connectionId).ToArray()
                };
            });
            return Task.FromResult(rv);
        });
    }

    public Task<TableState> AddRemoteConnectionToTableAsync(string tableId, string connectionId)
    {
        ConnectionsToTable.AddOrUpdate(connectionId, tableId, (_, _) => tableId);

        return WithLock(tableId, () =>
        {
            TableState rv = Tables.AddOrUpdate(tableId, new TableState(tableId, null), (_, existing) =>
            {
                return existing with
                {
                    RemoteConnections = existing.RemoteConnections.Append(connectionId).ToArray()
                };
            });
            return Task.FromResult(rv);
        });
    }

    public Task<TableState> AddTableConnectionAsync(string tableId, TableConfiguration tableConfiguration, string connectionId)
    {
        ConnectionsToTable.AddOrUpdate(connectionId, tableId, (_, _) => tableId);

        return WithLock(tableId, () =>
        {
            TableState rv = Tables.AddOrUpdate(tableId, new TableState(tableId, tableConfiguration) {  TableConnectionId = connectionId }, (_, existing) =>
            {
                return existing with
                {
                    TableConnectionId = connectionId,
                    TableConfiguration = tableConfiguration
                };
            });
            return Task.FromResult(rv);
        });
    }

    public Task<TableState?> GetTableStateAsync(string connectionId)
    {
        return WithConnection(connectionId, table => table);
    }

    public Task<TableState?> UpdateTableStateAsync(string connectionId, Func<TableState, TableState> updateTableState)
    {
        return WithConnection(connectionId, tableState => 
        {
            if (tableState.IsRemote(connectionId))
            {
                return updateTableState(tableState);
            }
            return tableState;
        });
    }

    private Task<TableState?> WithConnection(string connectionId, Func<TableState, TableState?> updateTable)
    {
        if (ConnectionsToTable.TryGetValue(connectionId, out string? tableId))
        {
            return WithExistingTable(tableId, table => updateTable(table));
        }
        return Task.FromResult<TableState?>(null);
    }

    private Task<TableState?> WithExistingTable(string tableId, Func<TableState, TableState?> updateTable)
    {
        return WithLock(tableId, () =>
        {
            if (Tables.TryGetValue(tableId, out TableState? existingTable))
            {
                TableState? updatedTable = updateTable(existingTable);
                if (updatedTable is not null)
                {
                    if (Tables.TryUpdate(tableId, updatedTable, existingTable))
                    {
                        return Task.FromResult<TableState?>(updatedTable);
                    }
                    else
                    {
                        return Task.FromResult<TableState?>(existingTable);
                    }
                }
                else
                {
                    Tables.TryRemove(tableId, out _);
                }
            }
            return Task.FromResult<TableState?>(null);
        });
    }

    private async Task<T> WithLock<T>(string tableId, Func<Task<T>> action)
    {
        SemaphoreSlim tableLock = TableLocks.AddOrUpdate(tableId, new SemaphoreSlim(1, 1), (_, existing) => existing);

        await tableLock.WaitAsync();

        try
        {
            return await action();
        }
        finally
        {
            tableLock.Release();
        }
    }


}
