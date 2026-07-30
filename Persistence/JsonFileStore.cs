using System.Text.Json;

namespace NuGetPackageExplorer.Persistence;

public class JsonFileStore<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonFileStore(string fileName)
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGetPackageExplorer");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, fileName);
    }

    public async Task<T?> LoadAsync()
    {
        if (!File.Exists(_filePath)) return default;
        await using FileStream stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<T>(stream, _options);
    }

    public async Task SaveAsync(T value)
    {
        await using FileStream stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, value, _options);
    }
}
