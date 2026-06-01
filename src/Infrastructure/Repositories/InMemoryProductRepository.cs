using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _store = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var p) ? p : null);

    public Task<IEnumerable<Product>> GetAllAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.Where(p => p.IsAvailable));

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _store[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _store[product.Id] = product;
        return Task.CompletedTask;
    }
}
