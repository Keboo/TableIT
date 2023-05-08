namespace TableIT.Shared;

public interface ITableConnection : IBaseTableConnection
{
    TableConfiguration TableConfiguration { get; set; }

    event Action<TableConfiguration>? TableConfigurationUpdated;
    event Action? ZoomToFit;
    event Action<float>? Zoom;
    event Action<int>? Rotate;
    event Action<int?, int?> Pan;
}
