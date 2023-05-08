namespace TableIT.Shared;

public interface IBaseTableConnection
{
    bool IsConnected { get; }

    Task DisconnectAsync();
    Task<bool> ConnectAsync(string tableId);

    Task<TableConfiguration> GetTableConfigurationAsync();
}
