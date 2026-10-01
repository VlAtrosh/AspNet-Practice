using MyMvcApp.Models;

namespace MyMvcApp.Services;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken ct = default);

    Task<Product> AddAsync(Product product, CancellationToken ct = default);

    Task<Product?> UpdateAsync(Product product, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}