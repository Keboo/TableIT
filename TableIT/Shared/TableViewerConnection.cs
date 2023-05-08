using Microsoft.AspNetCore.SignalR.Client;

namespace TableIT.Shared;

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
