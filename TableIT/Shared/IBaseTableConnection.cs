namespace TableIT.Shared;

public interface IBaseTableConnection
{
    bool IsConnected { get; }
    string? TableId { get; }

    Task DisconnectAsync();
    Task<bool> ConnectAsync(string tableId);

    Task<TableConfiguration> GetTableConfigurationAsync();
}
