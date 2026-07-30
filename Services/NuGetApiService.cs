using System.Net;
using System.Text.Json;
using NuGetPackageExplorer.Models;

namespace NuGetPackageExplorer.Services;

public class NuGetApiService
{
    private const string ServiceIndexUrl = "https://api.nuget.org/v3/index.json";
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private string? _searchUrl;
    private string? _registrationUrl;

    public NuGetApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<PackageSearchResult>> SearchAsync(string query, int take = 15)
    {
        await EnsureEndpointsAsync();
        string url = $"{_searchUrl}?q={Uri.EscapeDataString(query)}&skip=0&take={take}&prerelease=false&semVerLevel=2.0.0";
        SearchResponse response = await GetJsonAsync<SearchResponse>(url);
        return response.Data;
    }

    public async Task<List<PackageSearchResult>> BrowseAsync(string searchTerm, int take = 15)
    {
        await EnsureEndpointsAsync();
        string url = $"{_searchUrl}?q={Uri.EscapeDataString(searchTerm)}&skip=0&take={take}&prerelease=false&semVerLevel=2.0.0";
        SearchResponse response = await GetJsonAsync<SearchResponse>(url);
        return response.Data;
    }

    public async Task<PackageDetails?> GetPackageDetailsAsync(string packageId)
    {
        List<PackageSearchResult> exactSearch = await SearchAsync($"packageid:{packageId}", 20);
        PackageSearchResult? searchResult = exactSearch.FirstOrDefault(item =>
            item.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        if (searchResult is null) return null;

        List<PackageMetadata> versions = await GetAllVersionsAsync(searchResult.Id);
        PackageMetadata? latest = versions
            .Where(item => item.Listed is not false && item.Published?.Year != 1900)
            .LastOrDefault() ?? versions.LastOrDefault();
        if (latest is null) return null;

        return new PackageDetails
        {
            SearchResult = searchResult,
            LatestMetadata = latest,
            Versions = versions
        };
    }

    private async Task<List<PackageMetadata>> GetAllVersionsAsync(string packageId)
    {
        await EnsureEndpointsAsync();
        string url = $"{_registrationUrl}{Uri.EscapeDataString(packageId.ToLowerInvariant())}/index.json";
        RegistrationIndex index = await GetJsonAsync<RegistrationIndex>(url);
        List<RegistrationLeaf> leaves = new();

        foreach (RegistrationPage page in index.Items)
        {
            if (page.Items is not null)
            {
                leaves.AddRange(page.Items);
            }
            else if (!string.IsNullOrWhiteSpace(page.Id))
            {
                RegistrationPage loadedPage = await GetJsonAsync<RegistrationPage>(page.Id);
                if (loadedPage.Items is not null) leaves.AddRange(loadedPage.Items);
            }
        }

        List<PackageMetadata> versions = new();
        foreach (RegistrationLeaf leaf in leaves)
        {
            if (leaf.CatalogEntry.ValueKind != JsonValueKind.Object) continue;
            PackageMetadata? metadata = leaf.CatalogEntry.Deserialize<PackageMetadata>(_jsonOptions);
            if (metadata is not null) versions.Add(metadata);
        }

        return versions;
    }

    private async Task EnsureEndpointsAsync()
    {
        if (_searchUrl is not null && _registrationUrl is not null) return;
        ServiceIndex index = await GetJsonAsync<ServiceIndex>(ServiceIndexUrl);

        _searchUrl = FindResource(index, "SearchQueryService")
            ?? throw new InvalidOperationException("NuGet did not advertise a search service.");
        _registrationUrl = FindResource(index, "RegistrationsBaseUrl/3.6.0")
            ?? FindResource(index, "RegistrationsBaseUrl")
            ?? throw new InvalidOperationException("NuGet did not advertise a registration service.");

        if (!_registrationUrl.EndsWith('/')) _registrationUrl += "/";
    }

    private static string? FindResource(ServiceIndex index, string typeStart) =>
        index.Resources.FirstOrDefault(resource =>
            resource.Types.Any(type => type.StartsWith(typeStart, StringComparison.OrdinalIgnoreCase)))?.Id;

    private async Task<T> GetJsonAsync<T>(string url)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("The requested package could not be found.");
        }

        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        T? value = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
        return value ?? throw new InvalidOperationException("NuGet returned an empty or unreadable response.");
    }
}
