namespace TableIT.Shared;

public interface ITableViewerConnection : IBaseTableConnection
{
    event Action<TableConfiguration>? TableConfigurationUpdated;
}
