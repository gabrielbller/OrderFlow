using Domain.Customer.Entities;
using Domain.Customer.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _store = new();

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var c) ? c : null);

    public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.FirstOrDefault(c => c.Cpf.Value == cpf));

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.FirstOrDefault(c => c.Email.Value == email));

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _store[customer.Id] = customer;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _store[customer.Id] = customer;
        return Task.CompletedTask;
    }
}
