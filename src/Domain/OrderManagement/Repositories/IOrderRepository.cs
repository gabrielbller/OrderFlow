// =============================================================================
// DDD - Repository Interface (pertence ao domínio, não à infraestrutura)
// SOLID - DIP (Dependency Inversion Principle): classes de domínio dependem
//   desta ABSTRAÇÃO, não de implementações concretas (InMemory, EF Core, etc.)
// SOLID - ISP (Interface Segregation Principle): interface específica para
//   Order — não acumula operações de outros agregados.
// GRASP - Low Coupling: o domínio não sabe nada sobre como os dados são
//   persistidos; apenas declara o contrato necessário.
// =============================================================================

using Domain.OrderManagement.Entities;

namespace Domain.OrderManagement.Repositories;

/// <summary>
/// Contrato de repositório para o Aggregate Order.
/// Implementado na camada de infraestrutura.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
