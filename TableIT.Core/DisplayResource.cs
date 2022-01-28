using System.Text.Json.Serialization;

namespace TableIT.Core;

internal class DisplayResource
{
    [JsonPropertyName("resourceId")]
    public string? ResourceId { get; set; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
