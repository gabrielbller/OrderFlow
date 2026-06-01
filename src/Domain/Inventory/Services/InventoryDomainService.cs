// =============================================================================
// DDD - Domain Service: InventoryDomainService (Bounded Context: Inventory)
// Operações de estoque que envolvem lógica de domínio não pertencente ao
// Aggregate Product isolado.
// SOLID - SRP: responsabilidade única — gerenciar disponibilidade de estoque.
// SOLID - DIP: depende de IProductRepository (abstração).
// GRASP - High Cohesion: todos os métodos são coesos ao tema de estoque.
// =============================================================================

using Domain.Inventory.Repositories;

namespace Domain.Inventory.Services;

/// <summary>
/// Serviço de domínio para operações de estoque que coordenam múltiplos
/// produtos ou requerem consultas ao repositório.
/// </summary>
// Implementa IInventoryDomainService — métodos virtual permitem mock nos testes
public class InventoryDomainService : IInventoryDomainService
{
    // SOLID - DIP: depende de abstração
    private readonly IProductRepository _productRepository;

    public InventoryDomainService(IProductRepository productRepository)
    {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <summary>
    /// Verifica se há estoque disponível para a quantidade solicitada.
    /// virtual — permite override em mocks (Moq) durante testes unitários.
    /// </summary>
    public virtual async Task<bool> IsStockAvailableAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product is not null && product.IsAvailable && product.StockQuantity >= quantity;
    }

    /// <summary>
    /// Reserva estoque após confirmação de pagamento.
    /// virtual — permite override em mocks (Moq) durante testes unitários.
    /// </summary>
    public virtual async Task ReserveStockAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Produto {productId} não encontrado.");

        product.ReserveStock(quantity);
        await _productRepository.UpdateAsync(product, cancellationToken);
    }

    /// <summary>
    /// Libera estoque reservado em caso de cancelamento.
    /// virtual — permite override em mocks (Moq) durante testes unitários.
    /// </summary>
    public virtual async Task ReleaseStockAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Produto {productId} não encontrado.");

        product.ReplenishStock(quantity);
        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}
