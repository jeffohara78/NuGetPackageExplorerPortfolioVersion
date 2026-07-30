namespace NuGetPackageExplorer.Models;

public class PackageDetails
{
    public required PackageSearchResult SearchResult { get; init; }
    public required PackageMetadata LatestMetadata { get; init; }
    public List<PackageMetadata> Versions { get; init; } = [];
}

public class FavoritePackage
{
    public string Id { get; init; } = string.Empty;
    public DateTimeOffset SavedAt { get; init; }
}
