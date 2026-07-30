# NuGet Package Explorer

A .NET 8 console application that searches NuGet.org, displays package metadata, explores versions and dependencies, compares packages, and stores favorites locally.

## Run

1. Open `NuGetPackageExplorer.csproj` in Visual Studio 2022.
2. Confirm the .NET 8 SDK is installed.
3. Press **Ctrl+F5**.

No API key or NuGet account is required. An internet connection is required.

## Architecture

- `Program.cs` creates the application's dependencies.
- `Services/AppManager.cs` controls menus and user interaction.
- `Services/NuGetApiService.cs` calls the official NuGet V3 API.
- `Models` contains API and application data models.
- `Persistence` stores favorites as JSON in the user's local application-data folder.
- `Utilities` contains reusable input and display helpers.
