using System.Net.Http;
using NuGetPackageExplorer.Models;
using NuGetPackageExplorer.Utilities;

namespace NuGetPackageExplorer.Services;

public class AppManager
{
    private readonly NuGetApiService _api;
    private readonly FavoriteService _favorites;

    public AppManager(NuGetApiService api, FavoriteService favorites)
    {
        _api = api;
        _favorites = favorites;
    }

    public async Task RunAsync()
    {
        bool running = true;
        while (running)
        {
            ConsoleDisplay.Header("NuGet Package Explorer");
            Console.WriteLine("1. Search packages");
            Console.WriteLine("2. Browse useful package categories");
            Console.WriteLine("3. View a package by exact ID");
            Console.WriteLine("4. Compare two packages");
            Console.WriteLine("5. Manage favorites");
            Console.WriteLine("6. About NuGet and this app");
            Console.WriteLine("0. Exit\n");

            int choice = InputHelper.ReadNumber("Choose an option: ", 0, 6);
            try
            {
                switch (choice)
                {
                    case 1: await SearchPackagesAsync(); break;
                    case 2: await BrowseCategoriesAsync(); break;
                    case 3: await ViewExactPackageAsync(); break;
                    case 4: await ComparePackagesAsync(); break;
                    case 5: await ManageFavoritesAsync(); break;
                    case 6: ShowAbout(); break;
                    case 0: running = false; break;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nNuGet connection error: {ex.Message}");
                InputHelper.Pause();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nThe request took too long. Please try again.");
                InputHelper.Pause();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnable to complete the request: {ex.Message}");
                InputHelper.Pause();
            }
        }
    }

    private async Task SearchPackagesAsync()
    {
        ConsoleDisplay.Header("Search NuGet Packages");
        string query = InputHelper.ReadRequired("Search for a package or purpose (example: PDF, JSON, Excel): ");
        List<PackageSearchResult> results = await _api.SearchAsync(query);
        await ChooseResultAsync(results);
    }

    private async Task BrowseCategoriesAsync()
    {
        ConsoleDisplay.Header("Browse Package Categories");
        string[] labels = ["JSON and serialization", "Databases", "Testing", "PDF creation", "Excel files", "Logging", "Web APIs", "Dependency injection"];
        string[] queries = ["json serialization", "database sqlite", "testing", "pdf", "excel", "logging", "web api", "dependency injection"];
        for (int i = 0; i < labels.Length; i++) Console.WriteLine($"{i + 1}. {labels[i]}");
        Console.WriteLine("0. Back");
        int choice = InputHelper.ReadNumber("Choose a category: ", 0, labels.Length);
        if (choice == 0) return;
        List<PackageSearchResult> results = await _api.BrowseAsync(queries[choice - 1]);
        await ChooseResultAsync(results);
    }

    private async Task ChooseResultAsync(List<PackageSearchResult> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("\nNo packages matched that search.");
            InputHelper.Pause();
            return;
        }

        Console.WriteLine();
        ConsoleDisplay.SearchResults(results);
        Console.WriteLine("0. Back");
        int choice = InputHelper.ReadNumber("Select a package: ", 0, results.Count);
        if (choice > 0) await ViewPackageAsync(results[choice - 1].Id);
    }

    private async Task ViewExactPackageAsync()
    {
        ConsoleDisplay.Header("View Package by Exact ID");
        string id = InputHelper.ReadRequired("Package ID (example: Newtonsoft.Json): ");
        await ViewPackageAsync(id);
    }

    private async Task ViewPackageAsync(string id)
    {
        PackageDetails? details = await _api.GetPackageDetailsAsync(id);
        if (details is null)
        {
            Console.WriteLine("\nThat exact package could not be found.");
            InputHelper.Pause();
            return;
        }

        bool viewing = true;
        while (viewing)
        {
            ConsoleDisplay.Header(details.LatestMetadata.Id);
            ConsoleDisplay.Details(details);
            Console.WriteLine("\n1. View latest-version dependencies");
            Console.WriteLine("2. View recent versions");
            Console.WriteLine("3. Save to favorites");
            Console.WriteLine("0. Back");
            int choice = InputHelper.ReadNumber("Choose an option: ", 0, 3);
            switch (choice)
            {
                case 1:
                    ConsoleDisplay.Header($"Dependencies - {details.LatestMetadata.Id}");
                    ConsoleDisplay.Dependencies(details.LatestMetadata);
                    InputHelper.Pause();
                    break;
                case 2:
                    ConsoleDisplay.Header($"Recent Versions - {details.LatestMetadata.Id}");
                    foreach (PackageMetadata version in details.Versions.TakeLast(20).Reverse())
                        Console.WriteLine($"{version.Version,-22} {version.Published?.ToString("yyyy-MM-dd") ?? "Unknown"}");
                    InputHelper.Pause();
                    break;
                case 3:
                    Console.WriteLine(await _favorites.AddAsync(details.LatestMetadata.Id)
                        ? "\nPackage saved to favorites."
                        : "\nThat package is already a favorite.");
                    InputHelper.Pause();
                    break;
                case 0: viewing = false; break;
            }
        }
    }

    private async Task ComparePackagesAsync()
    {
        ConsoleDisplay.Header("Compare Packages");
        string firstId = InputHelper.ReadRequired("First exact package ID: ");
        string secondId = InputHelper.ReadRequired("Second exact package ID: ");
        PackageDetails? first = await _api.GetPackageDetailsAsync(firstId);
        PackageDetails? second = await _api.GetPackageDetailsAsync(secondId);
        if (first is null || second is null)
        {
            Console.WriteLine("\nOne or both package IDs could not be found.");
            InputHelper.Pause();
            return;
        }

        ConsoleDisplay.Header("Package Comparison");
        Console.WriteLine($"{"Field",-18}{first.LatestMetadata.Id,-26}{second.LatestMetadata.Id}");
        Console.WriteLine(new string('-', 72));
        CompareRow("Latest version", first.LatestMetadata.Version, second.LatestMetadata.Version);
        CompareRow("Downloads", first.SearchResult.TotalDownloads.ToString("N0"), second.SearchResult.TotalDownloads.ToString("N0"));
        CompareRow("Verified", first.SearchResult.Verified ? "Yes" : "No", second.SearchResult.Verified ? "Yes" : "No");
        CompareRow("Authors", first.LatestMetadata.Authors, second.LatestMetadata.Authors);
        CompareRow("License", first.LatestMetadata.LicenseExpression, second.LatestMetadata.LicenseExpression);
        CompareRow("Version count", first.Versions.Count.ToString("N0"), second.Versions.Count.ToString("N0"));
        CompareRow("Dependencies", CountDependencies(first).ToString(), CountDependencies(second).ToString());
        InputHelper.Pause();
    }

    private async Task ManageFavoritesAsync()
    {
        ConsoleDisplay.Header("Favorite Packages");
        List<FavoritePackage> favorites = await _favorites.GetAllAsync();
        if (favorites.Count == 0)
        {
            Console.WriteLine("No packages have been saved yet.");
            InputHelper.Pause();
            return;
        }
        for (int i = 0; i < favorites.Count; i++)
            Console.WriteLine($"{i + 1}. {favorites[i].Id} (saved {favorites[i].SavedAt:yyyy-MM-dd})");
        Console.WriteLine("\n1. Open a favorite");
        Console.WriteLine("2. Remove a favorite");
        Console.WriteLine("0. Back");
        int action = InputHelper.ReadNumber("Choose an option: ", 0, 2);
        if (action == 0) return;
        int selection = InputHelper.ReadNumber("Choose a package number: ", 1, favorites.Count);
        FavoritePackage selected = favorites[selection - 1];
        if (action == 1) await ViewPackageAsync(selected.Id);
        else
        {
            await _favorites.RemoveAsync(selected.Id);
            Console.WriteLine("\nFavorite removed.");
            InputHelper.Pause();
        }
    }

    private static void ShowAbout()
    {
        ConsoleDisplay.Header("About This App");
        Console.WriteLine("NuGet is the package manager used by .NET developers to find and install reusable libraries.");
        Console.WriteLine("This explorer reads public NuGet.org data but does not install, alter, or download packages.");
        Console.WriteLine("\nThe app demonstrates REST APIs, JSON deserialization, async programming, persistence,");
        Console.WriteLine("service discovery, dependency data, error handling, and clean project organization.");
        InputHelper.Pause();
    }

    private static int CountDependencies(PackageDetails details) =>
        details.LatestMetadata.DependencyGroups.Sum(group => group.Dependencies.Count);

    private static void CompareRow(string field, string first, string second) =>
        Console.WriteLine($"{field,-18}{Short(first),-26}{Short(second)}");

    private static string Short(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Not provided" : value.Length <= 24 ? value : value[..21] + "...";
}
