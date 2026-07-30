using System.Text.Json.Serialization;

namespace NuGetPackageExplorer.Models;

public class ServiceIndex
{
    [JsonPropertyName("resources")]
    public List<ServiceResource> Resources { get; init; } = new();
}

public class ServiceResource
{
    [JsonPropertyName("@id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("@type")]
    public object? TypeValue { get; init; }
        
    public IEnumerable<string> Types => TypeValue switch
    {
        string text => new[] { text },

        System.Text.Json.JsonElement element
            when element.ValueKind == System.Text.Json.JsonValueKind.String =>
            new[] { element.GetString() ?? string.Empty },

        System.Text.Json.JsonElement element
            when element.ValueKind == System.Text.Json.JsonValueKind.Array =>
            element.EnumerateArray()
                .Where(item =>
                    item.ValueKind == System.Text.Json.JsonValueKind.String)
                .Select(item => item.GetString() ?? string.Empty),

        _ => Enumerable.Empty<string>()
    };
}
