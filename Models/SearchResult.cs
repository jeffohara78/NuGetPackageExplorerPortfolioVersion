using System.Text.Json.Serialization;

namespace NuGetPackageExplorer.Models;

public class SearchResponse
{
    [JsonPropertyName("totalHits")]
    public int TotalHits { get; init; }

    [JsonPropertyName("data")]
    public List<PackageSearchResult> Data { get; init; } = new();
}

public class PackageSearchResult
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("authors")]
    public List<string> Authors { get; init; } = new();

    [JsonPropertyName("totalDownloads")]
    public long TotalDownloads { get; init; }

    [JsonPropertyName("verified")]
    public bool Verified { get; init; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; init; } = new();

    [JsonPropertyName("versions")]
    public List<SearchVersion> Versions { get; init; } = new();
}

public class SearchVersion
{
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    [JsonPropertyName("downloads")]
    public long Downloads { get; init; }
}
