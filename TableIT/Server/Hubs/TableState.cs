namespace TableIT.Server.Hubs;

public record class TableState(string TableId)
{
    public string? TableConnectionId { get; set; }

    public IReadOnlyList<string> RemoteConnections { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> ViewerConnections { get; set; } = Array.Empty<string>();

    public bool IsRemote(string connectionId) => RemoteConnections.Contains(connectionId);
}
