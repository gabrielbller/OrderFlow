// =============================================================================
// DDD - Factory
// A Factory centraliza a lógica de criação complexa de Aggregates.
// Diferença de Domain Service:
//   - Factory: responsável por CRIAR um objeto complexo (construção/montagem)
//   - Domain Service: coordena operações que envolvem múltiplos Aggregates
//     ou regras que não pertencem a um único Aggregate.
//
// SOLID - SRP: OrderFactory só cria pedidos — não tem lógica de negócio.
// SOLID - DIP: depende da interface IProductRepository para validação,
//   não da implementação concreta.
// GRASP - Creator: a Factory é quem tem as informações necessárias para
//   criar Order, centralizando esse conhecimento.
// =============================================================================

using Domain.Customer.Repositories;
using Domain.Inventory.Repositories;
using Domain.OrderManagement.Entities;
using Domain.OrderManagement.ValueObjects;

namespace Domain.OrderManagement.Factories;

/// <summary>
/// Factory responsável por criar instâncias de <see cref="Order"/> de forma
/// consistente, aplicando validações de pré-condição antes da criação.
/// </summary>
public class OrderFactory
{
    // SOLID - DIP: depende de abstrações (interfaces), não de concretos
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository  _productRepository;

    public OrderFactory(ICustomerRepository customerRepository,
                        IProductRepository productRepository)
    {
        _customerRepository = customerRepository
            ?? throw new ArgumentNullException(nameof(customerRepository));
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <summary>
    /// Cria um novo Pedido após validar que o cliente e os produtos existem.
    /// </summary>
    public async Task<Order> CreateAsync(
        Guid customerId,
        Address deliveryAddress,
        IEnumerable<(Guid ProductId, int Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        // Valida existência do cliente
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException($"Cliente {customerId} não encontrado.");

        if (!customer.IsActive)
            throw new InvalidOperationException("Cliente inativo não pode realizar pedidos.");

        var order = new Order(Guid.NewGuid(), customerId, deliveryAddress);

        // Adiciona os itens ao pedido
        foreach (var (productId, quantity) in items)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException($"Produto {productId} não encontrado.");

            if (!product.IsAvailable)
                throw new InvalidOperationException($"Produto '{product.Name}' não está disponível.");

            order.AddItem(product.Id, product.Name, quantity, product.Price);
        }

        return order;
    }
}
