using Microsoft.AspNetCore.SignalR.Client;

namespace TableIT.Shared;

public class TableConnection : BaseTableConnection, ITableConnection
{
    public event Action<TableConfiguration>? TableConfigurationUpdated;
    public event Action? ZoomToFit;
    public event Action<float>? Zoom;
    public event Action<int>? Rotate;
    public event Action<int?, int?>? Pan;

    public TableConfiguration TableConfiguration { get; set; } = new(null, null);

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

            HubConnection.On(TableConfigurationUpdatedMethodName, (TableConfiguration tableConfiguration) =>
            {
                TableConfiguration = tableConfiguration;
                TableConfigurationUpdated?.Invoke(tableConfiguration);
            });

            HubConnection.On(ZoomToFitMethodName, () =>
            {
                ZoomToFit?.Invoke();
            });

            HubConnection.On(ZoomMethodName, (float zoomAdjustment) =>
            {
                Zoom?.Invoke(zoomAdjustment);
            });

            HubConnection.On(RotateMethodName, (int degrees) =>
            {
                Rotate?.Invoke(degrees);
            });

            HubConnection.On(PanMethodName, (int? horizontalOffset, int? verticalOffset) =>
            {
                Pan?.Invoke(horizontalOffset, verticalOffset);
            });
        }
        return false;
    }
}
