// Implementação em memória do IOrderRepository — usada em testes e demos.
// SOLID - DIP: implementa a interface do domínio.

using Domain.OrderManagement.Entities;
using Domain.OrderManagement.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _store = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var order) ? order : null);

    public Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.Where(o => o.CustomerId == customerId));

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.ContainsKey(id));
}
