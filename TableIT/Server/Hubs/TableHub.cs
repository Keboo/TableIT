using Microsoft.AspNetCore.SignalR;
using TableIT.Shared;

namespace TableIT.Server.Hubs;

public class TableHub : Hub
{
    public TableHub(ITableManager tableManager, ILogger<TableHub> logger)
    {
        TableManager = tableManager;
        Logger = logger;
    }

    public ITableManager TableManager { get; }
    public ILogger<TableHub> Logger { get; }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(BaseTableConnection.ConnectToTableMethodName)]
    public async Task<bool> ConnectViewerToTableAsync(string tableId)
    {
        TableState _ = await TableManager.AddViewerConnectionToTableAsync(tableId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        Logger.UserConnectedToTable(tableId, "Viewer", Context.ConnectionId);
        return true;
    }

    [HubMethodName(BaseTableConnection.ConnectRemoteToTableMethodName)]
    public async Task<bool> ConnectRemoteToTableAsync(string tableId)
    {
        TableState _ = await TableManager.AddRemoteConnectionToTableAsync(tableId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        Logger.UserConnectedToTable(tableId, "Remote", Context.ConnectionId);
        return true;
    }

    [HubMethodName(BaseTableConnection.ConnectAsTableMethodName)]
    public async Task ConnectAsTableAsync(string tableId, TableConfiguration tableConfiguration)
    {
        TableState _ = await TableManager.AddTableConnectionAsync(tableId, tableConfiguration, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        Logger.UserConnectedToTable(tableId, "Table", Context.ConnectionId);
        await Clients.OthersInGroup(tableId).SendAsync(BaseTableConnection.TableConfigurationUpdatedMethodName, tableConfiguration);
    }

    [HubMethodName(BaseTableConnection.GetTableConfigurationMethodName)]
    public async Task<TableConfiguration?> GetTableConfigurationAsync()
    {
        if (await TableManager.GetTableStateAsync(Context.ConnectionId) is { } tableState)
        {
            Logger.GetTableConfiguration(tableState.TableId, Context.ConnectionId);
            return tableState.TableConfiguration;
        }
        Logger.GetTableConfigurationNotFound(Context.ConnectionId);
        return null;
    }

    [HubMethodName(BaseTableConnection.SetCurrentImageMethodName)]
    public async Task<TableConfiguration?> SetCurrentImageAsync(string resourceId)
    {
        if (await TableManager.UpdateTableStateAsync(Context.ConnectionId, tableState =>
        {
            return tableState with
            {
                TableConfiguration = (tableState.TableConfiguration ?? new(null, null)) with
                {
                    CurrentResourceId = resourceId
                }
            };
        }) is { } tableState)
        {
            await Clients.GroupExcept(tableState.TableId, tableState.RemoteConnections)
                .SendAsync(BaseTableConnection.TableConfigurationUpdatedMethodName, tableState.TableConfiguration);
            Logger.SetCurrentImage(tableState.TableId, resourceId, Context.ConnectionId);
            return tableState.TableConfiguration;
        }
        return null;
    }

    [HubMethodName(BaseTableConnection.ZoomMethodName)]
    public async Task ZoomAsync(float zoomAdjustment)
    {
        TableState? tableState = await TableManager.GetTableStateAsync(Context.ConnectionId);
        if (tableState?.RemoteConnections.Contains(Context.ConnectionId) == true)
        {
            await Clients.GroupExcept(tableState.TableId, tableState.RemoteConnections)
                .SendAsync(BaseTableConnection.ZoomMethodName, zoomAdjustment);
        }
    }

    [HubMethodName(BaseTableConnection.ZoomToFitMethodName)]
    public async Task ZoomToFitAsync()
    {
        TableState? tableState = await TableManager.GetTableStateAsync(Context.ConnectionId);
        if (tableState?.RemoteConnections.Contains(Context.ConnectionId) == true)
        {
            await Clients.GroupExcept(tableState.TableId, tableState.RemoteConnections)
                .SendAsync(BaseTableConnection.ZoomToFitMethodName);
        }
    }

    [HubMethodName(BaseTableConnection.RotateMethodName)]
    public async Task RotateAsync(int degrees)
    {
        TableState? tableState = await TableManager.GetTableStateAsync(Context.ConnectionId);
        if (tableState?.RemoteConnections.Contains(Context.ConnectionId) == true)
        {
            await Clients.GroupExcept(tableState.TableId, tableState.RemoteConnections)
                .SendAsync(BaseTableConnection.RotateMethodName, degrees);
        }
    }

    [HubMethodName(BaseTableConnection.PanMethodName)]
    public async Task PanAsync(int? horizontalOffset, int? verticalOffset)
    {
        TableState? tableState = await TableManager.GetTableStateAsync(Context.ConnectionId);
        if (tableState?.RemoteConnections.Contains(Context.ConnectionId) == true)
        {
            await Clients.GroupExcept(tableState.TableId, tableState.RemoteConnections)
                .SendAsync(BaseTableConnection.PanMethodName, horizontalOffset, verticalOffset);
        }
    }
}
