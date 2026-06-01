// SOLID - DIP: abstração de repositório de cliente.

using CustomerEntity = Domain.Customer.Entities.Customer;

namespace Domain.Customer.Repositories;

/// <summary>
/// Contrato de repositório para o Aggregate Customer.
/// </summary>
public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerEntity?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerEntity customer, CancellationToken cancellationToken = default);
}
