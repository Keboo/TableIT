namespace TableIT.Shared.Resources;

public class ResourceData
{
    public string Id { get; }
    public string? Version { get; }
    public double HorizontalOffset { get; set; }
    public double VerticalOffset { get; set; }
    public float ZoomFactor { get; set; }

    public bool IsCurrent { get; set; }

    public ResourceData(string id, string? version)
    {
        Id = id;
        Version = version;
    }
}
