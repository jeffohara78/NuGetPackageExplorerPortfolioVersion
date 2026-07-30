using NuGetPackageExplorer.Models;
using NuGetPackageExplorer.Persistence;

namespace NuGetPackageExplorer.Services;

public class FavoriteService
{
    private readonly JsonFileStore<List<FavoritePackage>> _store = new("favorites.json");

    public async Task<List<FavoritePackage>> GetAllAsync() => await _store.LoadAsync() ?? new();

    public async Task<bool> AddAsync(string packageId)
    {
        List<FavoritePackage> favorites = await GetAllAsync();
        if (favorites.Any(item => item.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        favorites.Add(new FavoritePackage { Id = packageId, SavedAt = DateTimeOffset.Now });
        await _store.SaveAsync(favorites.OrderBy(item => item.Id).ToList());
        return true;
    }

    public async Task<bool> RemoveAsync(string packageId)
    {
        List<FavoritePackage> favorites = await GetAllAsync();
        int removed = favorites.RemoveAll(item => item.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        if (removed == 0) return false;
        await _store.SaveAsync(favorites);
        return true;
    }
}
