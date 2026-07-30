using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetPackageExplorer.Models;

public class RegistrationIndex
{
    [JsonPropertyName("items")]
    public List<RegistrationPage> Items { get; init; } = new();
}

public class RegistrationPage
{
    [JsonPropertyName("@id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("items")]
    public List<RegistrationLeaf>? Items { get; init; }
}

public class RegistrationLeaf
{
    [JsonPropertyName("catalogEntry")]
    public JsonElement CatalogEntry { get; init; }
}

public class PackageMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    [JsonPropertyName("authors")]
    public JsonElement AuthorsValue { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("published")]
    public DateTimeOffset? Published { get; init; }

    [JsonPropertyName("projectUrl")]
    public string ProjectUrl { get; init; } = string.Empty;

    [JsonPropertyName("licenseExpression")]
    public string LicenseExpression { get; init; } = string.Empty;

    [JsonPropertyName("licenseUrl")]
    public string LicenseUrl { get; init; } = string.Empty;

    [JsonPropertyName("tags")]
    public JsonElement TagsValue { get; init; }

    [JsonPropertyName("dependencyGroups")]
    public List<DependencyGroup> DependencyGroups { get; init; } = new();
    [JsonPropertyName("deprecation")]
    public DeprecationInfo? Deprecation { get; init; }

    [JsonPropertyName("listed")]
    public bool? Listed { get; init; }

    [JsonIgnore]
    public string Authors => JsonValueHelper.JoinStrings(AuthorsValue);

    [JsonIgnore]
    public string Tags => JsonValueHelper.JoinStrings(TagsValue);
}

public class DependencyGroup
{
    [JsonPropertyName("targetFramework")]
    public string TargetFramework { get; init; } = string.Empty;

    [JsonPropertyName("dependencies")]
    public List<PackageDependency> Dependencies { get; init; } = new();
}

public class PackageDependency
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("range")]
    public string Range { get; init; } = string.Empty;
}

public class DeprecationInfo
{
    [JsonPropertyName("reasons")]
    public List<string> Reasons { get; init; } = new();

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("alternatePackage")]
    public AlternatePackage? AlternatePackage { get; init; }
}

public class AlternatePackage
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}

internal static class JsonValueHelper
{
    public static string JoinStrings(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            return string.Join(", ", value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item)));
        }

        return string.Empty;
    }
}
