using Microsoft.AspNetCore.SignalR.Client;

namespace TableIT.Shared;

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

    public Task SendPanAsync(int? horizontalOffset, int? verticalOffset)
        => HubConnection.InvokeAsync(PanMethodName, horizontalOffset, verticalOffset);

    public Task SendRotateAsync(int degrees)
        => HubConnection.InvokeAsync(RotateMethodName, degrees);

    public Task SendZoomAsync(float zoomAdjustment)
        => HubConnection.InvokeAsync(ZoomMethodName, zoomAdjustment);


    public Task SendZoomToFitAsync()
        => HubConnection.InvokeAsync(ZoomToFitMethodName);


    public Task SetCurrentImage(string resourceId)
        => HubConnection.InvokeAsync(SetCurrentImageMethodName, resourceId);

}
