using Microsoft.AspNetCore.SignalR.Client;
using Polly;

namespace TableIT.Shared;

public interface IBaseTableConnection
{
    bool IsConnected { get; }

    Task DisconnectAsync();
    Task<bool> ConnectAsync(string tableId);

    Task<TableConfiguration> GetTableConfigurationAsync();
}

public interface ITableViewerConnection : IBaseTableConnection
{
    event Action<TableConfiguration>? TableConfigurationUpdated;
}

public interface ITableRemoteConnection : IBaseTableConnection
{
    
}

public interface ITableConnection : IBaseTableConnection
{
    TableConfiguration TableConfiguration { get; set; }
}

public class TableViewerConnection : BaseTableConnection, ITableViewerConnection
{
    public event Action<TableConfiguration>? TableConfigurationUpdated;

    public TableViewerConnection(Uri url) : base(url)
    {
    }

    public override async Task<bool> ConnectAsync(string tableId)
    {
        if (await base.ConnectAsync(tableId))
        {
            bool rv = await HubConnection.InvokeAsync<bool>(ConnectToTableMethodName, tableId);

            HubConnection.On(TableConfigurationUpdatedMethodName, (TableConfiguration tableConfiguration) => 
            {
                TableConfigurationUpdated?.Invoke(tableConfiguration);
            });

            return rv;
        }
        return false;
    }
}

public class TableRemoteConnection : BaseTableConnection, ITableRemoteConnection
{
    public TableRemoteConnection(Uri url) : base(url)
    {
    }

    public override async Task<bool> ConnectAsync(string tableId)
    {
        if (await base.ConnectAsync(tableId))
        {
            return await HubConnection.InvokeAsync<bool>(ConnectRemoteToTableMethodName, tableId);
        }
        return false;
    }
}

public class TableConnection : BaseTableConnection, ITableConnection
{
    public TableConfiguration TableConfiguration { get; set; } = new();

    public TableConnection(Uri url) : base(url)
    {
    }

    public override async Task<bool> ConnectAsync(string tableId)
    {
        if (await base.ConnectAsync(tableId))
        {
            await HubConnection.InvokeAsync(ConnectAsTableMethodName, tableId, TableConfiguration);

            //TODO: Clean these up
            HubConnection.On(GetTableConfigurationMethodName, () =>
            {
                return Task.FromResult(TableConfiguration);
            });
        }
        return false;
    }
}

public abstract class BaseTableConnection : IBaseTableConnection
{
    public const string GetTableConfigurationMethodName = "GetTableConfiguration";
    public const string TableConfigurationUpdatedMethodName = "TableConfigurationUpdated";

    public const string ConnectToTableMethodName = "ConnectToTable";
    public const string ConnectRemoteToTableMethodName = "ConnectRemoteToTable";
    public const string ConnectAsTableMethodName = "ConnectAsTable";

    protected HubConnection HubConnection { get; }

    public bool IsConnected => HubConnection.State == HubConnectionState.Connected;

    public Uri HubUrl { get; }
    
    public BaseTableConnection(Uri url)
    {
        HubUrl = url;
        HubConnection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options => { })
            .WithAutomaticReconnect()
            .Build();
    }

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<bool> ConnectAsync(string tableId)
    {
        var pauseBetweenFailures = TimeSpan.FromSeconds(5);
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(
                i => pauseBetweenFailures,
                (exception, timeSpan) =>
                {

                }
            );
        await retryPolicy.ExecuteAsync(() => HubConnection.StartAsync());
        return true;
    }

    public Task<TableConfiguration> GetTableConfigurationAsync()
        => HubConnection.InvokeAsync<TableConfiguration>(GetTableConfigurationMethodName);


}