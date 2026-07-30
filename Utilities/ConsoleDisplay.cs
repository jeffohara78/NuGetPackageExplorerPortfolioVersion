using NuGetPackageExplorer.Models;

namespace NuGetPackageExplorer.Utilities;

public static class ConsoleDisplay
{
    public static void Header(string title)
    {
        Console.Clear();
        Console.WriteLine(new string('=', 72));
        Console.WriteLine(title.ToUpperInvariant());
        Console.WriteLine(new string('=', 72));
    }

    public static void SearchResults(IReadOnlyList<PackageSearchResult> packages)
    {
        for (int i = 0; i < packages.Count; i++)
        {
            PackageSearchResult package = packages[i];
            string verified = package.Verified ? " [Verified]" : string.Empty;
            Console.WriteLine($"{i + 1,2}. {package.Id}{verified}");
            Console.WriteLine($"    Latest: {package.Version} | Downloads: {package.TotalDownloads:N0}");
            Console.WriteLine($"    {Shorten(package.Description, 100)}\n");
        }
    }

    public static void Details(PackageDetails details)
    {
        PackageMetadata item = details.LatestMetadata;
        Console.WriteLine($"Package:       {item.Id}");
        Console.WriteLine($"Latest:        {item.Version}");
        Console.WriteLine($"Authors:       {Fallback(item.Authors)}");
        Console.WriteLine($"Downloads:     {details.SearchResult.TotalDownloads:N0}");
        Console.WriteLine($"Verified:      {(details.SearchResult.Verified ? "Yes" : "No")}");
        Console.WriteLine($"Published:     {item.Published?.ToString("MMMM d, yyyy") ?? "Unknown"}");
        Console.WriteLine($"License:       {Fallback(item.LicenseExpression, item.LicenseUrl)}");
        Console.WriteLine($"Project URL:   {Fallback(item.ProjectUrl)}");
        Console.WriteLine($"Tags:          {Fallback(item.Tags)}");
        Console.WriteLine($"Versions:      {details.Versions.Count:N0}");
        Console.WriteLine($"\nDescription:\n{Fallback(item.Description, item.Summary)}");

        if (item.Deprecation is not null)
        {
            Console.WriteLine("\nWARNING: This package version is deprecated.");
            Console.WriteLine($"Reason: {string.Join(", ", item.Deprecation.Reasons)}");
            Console.WriteLine($"Message: {item.Deprecation.Message}");
            if (item.Deprecation.AlternatePackage is not null)
                Console.WriteLine($"Suggested replacement: {item.Deprecation.AlternatePackage.Id}");
        }
    }

    public static void Dependencies(PackageMetadata item)
    {
        if (item.DependencyGroups.Count == 0 || item.DependencyGroups.All(group => group.Dependencies.Count == 0))
        {
            Console.WriteLine("This package version declares no dependencies.");
            return;
        }

        foreach (DependencyGroup group in item.DependencyGroups)
        {
            Console.WriteLine($"\nTarget framework: {Fallback(group.TargetFramework, "Any")}");
            foreach (PackageDependency dependency in group.Dependencies)
                Console.WriteLine($"  - {dependency.Id} {dependency.Range}");
        }
    }

    private static string Shorten(string text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "No description provided." :
        text.Length <= max ? text : text[..(max - 3)] + "...";

    private static string Fallback(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "Not provided";
}
