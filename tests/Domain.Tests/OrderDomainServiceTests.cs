// =============================================================================
// TESTES UNITÁRIOS - OrderDomainService
// Uso de MOCKS (Moq) para isolar dependências externas:
//   - IOrderRepository     → Stub de persistência
//   - IProductRepository   → Stub de consulta de produto
//   - IPaymentService      → Mock do gateway de pagamento
//   - IInventoryDomainService → Mock do serviço de estoque (via interface)
// Demonstra: isolamento, mocks/stubs, testes negativos, F.I.R.S.T.
// =============================================================================

using Domain.Inventory.Services;
using Domain.Inventory.Repositories;
using Domain.OrderManagement.Entities;
using Domain.OrderManagement.Repositories;
using Domain.OrderManagement.Services;
using Domain.OrderManagement.ValueObjects;
using Domain.Payment.Services;
using Domain.Payment.ValueObjects;
using Moq;
using Xunit;

namespace Domain.Tests;

public class OrderDomainServiceTests
{
    // -------------------------------------------------------------------------
    // Mocks e Stubs — criados uma vez por instância de teste (ISOLATED)
    // -------------------------------------------------------------------------

    // STUB: simula repositório — nunca toca banco real (FAST)
    private readonly Mock<IOrderRepository>         _orderRepoMock      = new();
    // STUB: simula consulta de produto
    private readonly Mock<IProductRepository>       _productRepoMock    = new();
    // MOCK: gateway de pagamento — comportamento verificado com Verify()
    private readonly Mock<IPaymentService>          _paymentServiceMock = new();
    // MOCK: serviço de estoque — usa interface (SOLID DIP + testabilidade)
    private readonly Mock<IInventoryDomainService>  _inventoryMock      = new();

    private readonly OrderDomainService _sut;

    public OrderDomainServiceTests()
    {
        _sut = new OrderDomainService(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _paymentServiceMock.Object,
            _inventoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // Helper: cria um Order com um item já adicionado
    // -------------------------------------------------------------------------
    private static Order MakeOrderWithItem()
    {
        var address = new Address(
            "Rua A", "1", null, "Bairro", "Cidade", "SP", "00000-000", "Brasil");
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), address);
        order.AddItem(Guid.NewGuid(), "Produto Teste", 2, Money.FromBRL(100m));
        return order;
    }

    // =========================================================================
    // TESTES POSITIVOS — ConfirmOrderAsync
    // =========================================================================

    [Fact]
    public async Task ConfirmOrderAsync_EstoqueDisponivel_DeveConfirmarPedido()
    {
        // ARRANGE — STUB: estoque disponível
        _inventoryMock
            .Setup(s => s.IsStockAvailableAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), default))
            .ReturnsAsync(true);

        _orderRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var order = MakeOrderWithItem();

        // ACT
        await _sut.ConfirmOrderAsync(order);

        // ASSERT
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        _orderRepoMock.Verify(r => r.UpdateAsync(order, default), Times.Once);
    }

    // =========================================================================
    // TESTES POSITIVOS — ProcessPaymentAsync
    // =========================================================================

    [Fact]
    public async Task ProcessPaymentAsync_PagamentoBemSucedido_DevePagarPedido()
    {
        // ARRANGE — STUB gateway retorna sucesso
        _paymentServiceMock
            .Setup(p => p.ChargeAsync(
                It.IsAny<Guid>(), It.IsAny<Money>(), "valid_token", default))
            .ReturnsAsync(PaymentResult.Success("ch_abc123"));

        _inventoryMock
            .Setup(s => s.ReserveStockAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), default))
            .Returns(Task.CompletedTask);

        _orderRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var order = MakeOrderWithItem();
        order.Confirm();  // pré-condição: status Confirmed

        // ACT
        await _sut.ProcessPaymentAsync(order, "valid_token");

        // ASSERT
        Assert.Equal(OrderStatus.Paid, order.Status);

        // VERIFY — garante que o gateway foi chamado exatamente 1 vez
        _paymentServiceMock.Verify(p =>
            p.ChargeAsync(order.Id, It.IsAny<Money>(), "valid_token", default),
            Times.Once);
    }

    // =========================================================================
    // TESTES POSITIVOS — CancelOrderAsync
    // =========================================================================

    [Fact]
    public async Task CancelOrderAsync_PedidoPending_DeveCancelarSemLiberarEstoque()
    {
        _orderRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var order = MakeOrderWithItem();  // status: Pending

        // ACT
        await _sut.CancelOrderAsync(order);

        // ASSERT
        Assert.Equal(OrderStatus.Cancelled, order.Status);

        // Estoque NÃO liberado — pedido ainda não estava pago
        _inventoryMock.Verify(s =>
            s.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<int>(), default),
            Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_PedidoPago_DeveCancelarELiberarEstoque()
    {
        _paymentServiceMock
            .Setup(p => p.ChargeAsync(It.IsAny<Guid>(), It.IsAny<Money>(), "valid_token", default))
            .ReturnsAsync(PaymentResult.Success("ch_abc123"));
        _inventoryMock
            .Setup(s => s.ReserveStockAsync(It.IsAny<Guid>(), It.IsAny<int>(), default))
            .Returns(Task.CompletedTask);
        _inventoryMock
            .Setup(s => s.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<int>(), default))
            .Returns(Task.CompletedTask);
        _orderRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var order = MakeOrderWithItem();
        order.Confirm();
        await _sut.ProcessPaymentAsync(order, "valid_token"); // status: Paid

        // ACT
        await _sut.CancelOrderAsync(order);

        // ASSERT — cancelado e estoque liberado (pedido estava pago)
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _inventoryMock.Verify(s =>
            s.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<int>(), default),
            Times.Once);
    }

    // =========================================================================
    // TESTES NEGATIVOS
    // =========================================================================

    [Fact]
    public async Task ConfirmOrderAsync_EstoqueInsuficiente_DeveLancarExcecao()
    {
        // ARRANGE — STUB: sem estoque
        _inventoryMock
            .Setup(s => s.IsStockAvailableAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), default))
            .ReturnsAsync(false);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Domain.Inventory.Entities.Product?)null);

        var order = MakeOrderWithItem();

        // ACT + ASSERT — deve lançar exceção
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ConfirmOrderAsync(order));

        // Order permanece Pending — estado não mudou
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public async Task ProcessPaymentAsync_PagamentoFalhou_DeveLancarExcecaoESemMudarStatus()
    {
        // ARRANGE — STUB gateway retorna falha
        _paymentServiceMock
            .Setup(p => p.ChargeAsync(
                It.IsAny<Guid>(), It.IsAny<Money>(), "invalid_token", default))
            .ReturnsAsync(PaymentResult.Failure("Cartão recusado"));

        var order = MakeOrderWithItem();
        order.Confirm();  // pré-condição: Confirmed

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ProcessPaymentAsync(order, "invalid_token"));

        // Status não mudou — permanece Confirmed
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}
