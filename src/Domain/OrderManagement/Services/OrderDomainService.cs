// =============================================================================
// DDD - Domain Service: OrderDomainService
// Diferença de Factory:
//   - Domain Service: coordena operações que envolvem múltiplos Aggregates
//     ou regras de negócio que não pertencem exclusivamente a um único deles.
//   - Factory: só cria objetos, não contém regras de negócio de operação.
//
// SOLID - SRP: coordena o fluxo de confirmação e pagamento de pedidos,
//   orquestrando as chamadas entre Order, Inventory e Payment.
// SOLID - DIP: depende de interfaces (IOrderRepository, IPaymentService,
//   IProductRepository), nunca de implementações concretas.
// GRASP - Low Coupling: todas as dependências são injetadas — nenhum new()
//   de concreto aqui.
// GRASP - High Cohesion: todas as operações estão relacionadas ao fluxo
//   de processamento de pedidos.
// =============================================================================

using Domain.Inventory.Repositories;
using Domain.Inventory.Services;
using Domain.OrderManagement.Entities;
using Domain.OrderManagement.Repositories;
using Domain.Payment.Services;

namespace Domain.OrderManagement.Services;

/// <summary>
/// Serviço de domínio responsável por orquestrar operações que envolvem
/// múltiplos Aggregates no contexto de gestão de pedidos.
/// </summary>
public class OrderDomainService
{
    // SOLID - DIP: todas as dependências são abstrações
    private readonly IOrderRepository        _orderRepository;
    private readonly IProductRepository      _productRepository;
    private readonly IPaymentService         _paymentService;
    private readonly IInventoryDomainService _inventoryService; // usa interface para DIP + testabilidade

    public OrderDomainService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPaymentService paymentService,
        IInventoryDomainService inventoryService)
    {
        _orderRepository  = orderRepository  ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _paymentService   = paymentService   ?? throw new ArgumentNullException(nameof(paymentService));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    /// <summary>
    /// Confirma um pedido verificando disponibilidade de estoque.
    /// </summary>
    public async Task ConfirmOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        // Verifica disponibilidade de todos os itens no estoque
        foreach (var item in order.Items)
        {
            var available = await _inventoryService
                .IsStockAvailableAsync(item.ProductId, item.Quantity, cancellationToken);

            if (!available)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                throw new InvalidOperationException(
                    $"Estoque insuficiente para o produto '{product?.Name ?? item.ProductId.ToString()}'.");
            }
        }

        order.Confirm();
        await _orderRepository.UpdateAsync(order, cancellationToken);
    }

    /// <summary>
    /// Processa o pagamento de um pedido e atualiza seu status.
    /// </summary>
    public async Task ProcessPaymentAsync(Order order, string paymentToken,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.ChargeAsync(order.Id, order.Total, paymentToken, cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"Falha no pagamento: {result.ErrorMessage}");

        order.MarkAsPaid();

        // Reserva o estoque após pagamento bem-sucedido
        foreach (var item in order.Items)
        {
            await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity, cancellationToken);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
    }

    /// <summary>
    /// Cancela um pedido e libera o estoque se necessário.
    /// </summary>
    public async Task CancelOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var wasPaid = order.Status == Domain.OrderManagement.ValueObjects.OrderStatus.Paid;

        order.Cancel();

        // Se estava pago, libera o estoque reservado
        if (wasPaid)
        {
            foreach (var item in order.Items)
            {
                await _inventoryService.ReleaseStockAsync(item.ProductId, item.Quantity, cancellationToken);
            }
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
    }
}
