// SOLID - DIP: abstração de repositório de produto para desacoplar domínio da infraestrutura.

using Domain.Inventory.Entities;

namespace Domain.Inventory.Repositories;

/// <summary>
/// Contrato de repositório para o Aggregate Product.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAvailableAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
