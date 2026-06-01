// =============================================================================
// Interface do InventoryDomainService — permite mock nos testes (DIP + testabilidade)
// SOLID - DIP: testes e consumidores dependem desta abstração, não do concreto.
// =============================================================================

namespace Domain.Inventory.Services;

/// <summary>
/// Contrato do serviço de domínio de inventário.
/// Permite substituição por mock em testes unitários.
/// </summary>
public interface IInventoryDomainService
{
    Task<bool> IsStockAvailableAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default);

    Task ReserveStockAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default);

    Task ReleaseStockAsync(Guid productId, int quantity,
        CancellationToken cancellationToken = default);
}
