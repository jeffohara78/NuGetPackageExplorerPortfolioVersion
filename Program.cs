/* Jeff O'Hara 
 * 7/29/2026
 * 
 * NuGet Package Explorer is a C# console application that connects to the official NuGet API, allowing users to search, browse, compare,
 * and explore package information such as versions, downloads, dependencies, licenses, and deprecation notices. It demonstrates REST API 
 * consumption, asynchronous programming, JSON deserialization, clean architecture, and persistent JSON storage for favorite packages.
 */



using System.Net;
using NuGetPackageExplorer.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using HttpClientHandler handler = new()
{
    AutomaticDecompression = DecompressionMethods.GZip |
                             DecompressionMethods.Deflate |
                             DecompressionMethods.Brotli
};

using HttpClient httpClient = new(handler)
{
    Timeout = TimeSpan.FromSeconds(20)
}; 
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NuGetPackageExplorer/1.0");

NuGetApiService apiService = new(httpClient);
FavoriteService favoriteService = new();
AppManager appManager = new(apiService, favoriteService);

await appManager.RunAsync();
