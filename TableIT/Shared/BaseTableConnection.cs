using Microsoft.AspNetCore.SignalR.Client;
using Polly;

namespace TableIT.Shared;

public abstract class BaseTableConnection : IBaseTableConnection
{
    public const string GetTableConfigurationMethodName = "GetTableConfiguration";
    public const string TableConfigurationUpdatedMethodName = "TableConfigurationUpdated";

    public const string ConnectToTableMethodName = "ConnectToTable";
    public const string ConnectRemoteToTableMethodName = "ConnectRemoteToTable";
    public const string ConnectAsTableMethodName = "ConnectAsTable";

    public const string SetCurrentImageMethodName = "SetCurrentImage";
    public const string ZoomMethodName = "ZoomImage";
    public const string ZoomToFitMethodName = "ZoomToFit";
    public const string RotateMethodName = "RotateImage";
    public const string PanMethodName = "PanImage";

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
        return Task.CompletedTask;
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