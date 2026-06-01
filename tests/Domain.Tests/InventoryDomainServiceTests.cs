// =============================================================================
// TESTES UNITÁRIOS - InventoryDomainService (DDD - Domain Service)
// Uso de MOCK (Moq) do IProductRepository para isolar o serviço de domínio.
// Demonstra: isolamento, mocks/stubs, testes positivos e negativos, F.I.R.S.T.
// =============================================================================

using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Domain.Inventory.Services;
using Domain.Inventory.ValueObjects;
using Domain.OrderManagement.ValueObjects;
using Moq;
using Xunit;

namespace Domain.Tests;

public class InventoryDomainServiceTests
{
    // STUB do repositório — nenhuma persistência real (FAST, ISOLATED)
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly InventoryDomainService _sut;

    public InventoryDomainServiceTests()
    {
        _sut = new InventoryDomainService(_productRepoMock.Object);
    }

    private static Product MakeProduct(int stock) => new(
        Guid.NewGuid(), new ProductCode("SKU-001"), "Produto Teste",
        "Descrição", Money.FromBRL(50m), stock);

    // =========================================================================
    // IsStockAvailableAsync
    // =========================================================================

    [Fact]
    public async Task IsStockAvailableAsync_EstoqueSuficiente_DeveRetornarTrue()
    {
        var product = MakeProduct(stock: 10);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);

        var result = await _sut.IsStockAvailableAsync(product.Id, 5);

        Assert.True(result);
    }

    [Fact]
    public async Task IsStockAvailableAsync_EstoqueInsuficiente_DeveRetornarFalse()
    {
        var product = MakeProduct(stock: 2);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);

        var result = await _sut.IsStockAvailableAsync(product.Id, 5);

        Assert.False(result);
    }

    [Fact]
    public async Task IsStockAvailableAsync_ProdutoInexistente_DeveRetornarFalse()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        var result = await _sut.IsStockAvailableAsync(Guid.NewGuid(), 1);

        Assert.False(result);
    }

    // =========================================================================
    // ReserveStockAsync
    // =========================================================================

    [Fact]
    public async Task ReserveStockAsync_ProdutoExistente_DeveReduzirEstoqueEPersistir()
    {
        var product = MakeProduct(stock: 10);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);
        _productRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), default))
            .Returns(Task.CompletedTask);

        await _sut.ReserveStockAsync(product.Id, 3);

        Assert.Equal(7, product.StockQuantity);
        _productRepoMock.Verify(r => r.UpdateAsync(product, default), Times.Once);
    }

    [Fact]
    public async Task ReserveStockAsync_ProdutoInexistente_DeveLancarExcecao()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ReserveStockAsync(Guid.NewGuid(), 1));
    }

    // =========================================================================
    // ReleaseStockAsync
    // =========================================================================

    [Fact]
    public async Task ReleaseStockAsync_ProdutoExistente_DeveReporEstoqueEPersistir()
    {
        var product = MakeProduct(stock: 5);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);
        _productRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), default))
            .Returns(Task.CompletedTask);

        await _sut.ReleaseStockAsync(product.Id, 4);

        Assert.Equal(9, product.StockQuantity);
        _productRepoMock.Verify(r => r.UpdateAsync(product, default), Times.Once);
    }

    [Fact]
    public async Task ReleaseStockAsync_ProdutoInexistente_DeveLancarExcecao()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ReleaseStockAsync(Guid.NewGuid(), 1));
    }

    [Fact]
    public void Constructor_NullRepository_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentNullException>(() => new InventoryDomainService(null!));
    }
}
