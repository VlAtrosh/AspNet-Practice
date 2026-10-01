using System.Text.Json;
using MyMvcApp.Models;

namespace MyMvcApp.Services;

public class JsonProductRepository : IProductRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonProductRepository(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "Data", "products.json");
    }

    private async Task<List<Product>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return new();
        await using var stream = File.OpenRead(_filePath);
        var items = await JsonSerializer.DeserializeAsync<List<Product>>(stream, _opts, ct);
        return items ?? new();
    }

    private async Task SaveAsync(List<Product> items, CancellationToken ct)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, items, _opts, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        => await LoadAsync(ct);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => (await LoadAsync(ct)).FirstOrDefault(p => p.Id == id);

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken ct = default)
        => (await LoadAsync(ct))
            .Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var items = await LoadAsync(ct);
            product.Id = items.Count == 0 ? 1 : items.Max(p => p.Id) + 1;
            items.Add(product);
            await SaveAsync(items, ct);
            return product;
        }
        finally
        {
            _lock.Release();
        }
    }

    // НОВОЕ: обновление товара
    public async Task<Product?> UpdateAsync(Product product, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var items = await LoadAsync(ct);
            var existing = items.FirstOrDefault(p => p.Id == product.Id);
            if (existing is null) return null;

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Category = product.Category;

            await SaveAsync(items, ct);
            return existing;
        }
        finally
        {
            _lock.Release();
        }
    }

    // НОВОЕ: удаление товара
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var items = await LoadAsync(ct);
            var existing = items.FirstOrDefault(p => p.Id == id);
            if (existing is null) return false;

            items.Remove(existing);
            await SaveAsync(items, ct);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}