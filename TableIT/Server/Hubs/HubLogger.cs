namespace TableIT.Server.Hubs;

public static partial class HubLogger
{
    public const int BaseEventId = 1000;

    [LoggerMessage(
        EventId = BaseEventId + 1,
        Level = LogLevel.Information,
        Message = "User connected to table {TableId} as {UserType} on connection {ConnectionId}"
    )]
    public static partial void UserConnectedToTable(this ILogger logger, string tableId, string userType, string connectionId);

    [LoggerMessage(
        EventId = BaseEventId + 2,
        Level = LogLevel.Information,
        Message = "Retrieved table configuration for {TableId} on connection {ConnectionId}"
    )]
    public static partial void GetTableConfiguration(this ILogger logger, string tableId, string connectionId);

    [LoggerMessage(
        EventId = BaseEventId + 3,
        Level = LogLevel.Warning,
        Message = "Table configuration not found for connection {ConnectionId}"
    )]
    public static partial void GetTableConfigurationNotFound(this ILogger logger, string connectionId);

    [LoggerMessage(
        EventId = BaseEventId + 4,
        Level = LogLevel.Information,
        Message = "Table {TableId} image set to {ResourceId} not found for connection {ConnectionId}"
    )]
    public static partial void SetCurrentImage(this ILogger logger, string tableId, string resourceId, string connectionId);

}
