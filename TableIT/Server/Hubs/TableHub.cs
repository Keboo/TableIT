using Microsoft.AspNetCore.SignalR;
using TableIT.Shared;

namespace TableIT.Server.Hubs;

public class TableHub : Hub
{
    public TableHub(ITableManager tableManager)
    {
        TableManager = tableManager;
    }

    public ITableManager TableManager { get; }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(BaseTableConnection.ConnectToTableMethodName)]
    public async Task<bool> ConnectToTableAsync(string tableId)
    {
        TableState _ = await TableManager.AddViewerConnectionToTableAsync(tableId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        return true;
        //await Clients.Groups(tableId).SendAsync("", )
    }

    [HubMethodName(BaseTableConnection.ConnectRemoteToTableMethodName)]
    public async Task<bool> ConnectRemoteToTableAsync(string tableId)
    {
        TableState _ = await TableManager.AddRemoteConnectionToTableAsync(tableId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        return true;
        //await Clients.Groups(tableId).SendAsync("", )
    }

    [HubMethodName(BaseTableConnection.ConnectAsTableMethodName)]
    public async Task ConnectAsTableAsync(string tableId, TableConfiguration tableConfiguration)
    {
        TableState _ = await TableManager.AddTableConnectionAsync(tableId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        await Clients.OthersInGroup(tableId).SendAsync(BaseTableConnection.TableConfigurationUpdatedMethodName, tableConfiguration);
    }

    [HubMethodName(BaseTableConnection.GetTableConfigurationMethodName)]
    public async Task<TableConfiguration?> GetTableConfigurationAsync()
    {
        if (await TableManager.GetTableStateAsync(Context.ConnectionId) is { } tableState &&
            !string.IsNullOrWhiteSpace(tableState.TableConnectionId))
        {
            return await Clients.Client(tableState.TableConnectionId)
                .InvokeAsync<TableConfiguration>(BaseTableConnection.GetTableConfigurationMethodName, Context.ConnectionAborted);
        }
        return null;
    }
}
