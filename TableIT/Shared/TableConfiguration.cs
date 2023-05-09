namespace TableIT.Shared;

public record class TableConfiguration(string? CurrentResourceId, CompassConfiguration? Compass)
{
    public static TableConfiguration Empty => new(null, null);
}
