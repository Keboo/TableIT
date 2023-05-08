namespace TableIT.Shared;

public interface ITableRemoteConnection : IBaseTableConnection
{
    Task SetCurrentImage(string resourceId);
    Task SendZoomToFitAsync();
    Task SendZoomAsync(float zoomAdjustment);
    Task SendRotateAsync(int degrees);
    Task SendPanAsync(int? horizontalOffset, int? verticalOffset);
}
