// =============================================================================
// TESTES UNITÁRIOS - OrderFactory (DDD - Factory)
// Uso de MOCKS (Moq) para isolar a Factory das implementações de repositório:
//   - ICustomerRepository → Stub de consulta de cliente
//   - IProductRepository  → Stub de consulta de produto
// Demonstra: isolamento, mocks/stubs, testes negativos, F.I.R.S.T.
// =============================================================================

using Domain.Customer.Repositories;
using Domain.Customer.ValueObjects;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Domain.Inventory.ValueObjects;
using Domain.OrderManagement.Factories;
using Domain.OrderManagement.ValueObjects;
using Moq;
using Xunit;
using CustomerEntity = Domain.Customer.Entities.Customer;

namespace Domain.Tests;

public class OrderFactoryTests
{
    // STUBS — isolam a Factory de qualquer infraestrutura real (FAST, ISOLATED)
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IProductRepository>  _productRepoMock  = new();
    private readonly OrderFactory _sut;

    public OrderFactoryTests()
    {
        _sut = new OrderFactory(_customerRepoMock.Object, _productRepoMock.Object);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private static Address MakeAddress() =>
        new("Rua A", "1", null, "Bairro", "Cidade", "SP", "00000-000", "Brasil");

    private static CustomerEntity MakeCustomer() => new(
        Guid.NewGuid(), "Gabriel Bller",
        new Email("gabriel@email.com"), new Cpf("529.982.247-25"),
        new DateTime(1990, 1, 1));

    private static Product MakeProduct(int stock = 10) => new(
        Guid.NewGuid(), new ProductCode("SKU-001"), "Produto Teste",
        "Descrição", Money.FromBRL(50m), stock);

    // =========================================================================
    // TESTE POSITIVO
    // =========================================================================

    [Fact]
    public async Task CreateAsync_ClienteEProdutoValidos_DeveCriarPedidoComItens()
    {
        var customer = MakeCustomer();
        var product  = MakeProduct();

        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(customer);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(product);

        var items = new[] { (product.Id, 2) };

        var order = await _sut.CreateAsync(customer.Id, MakeAddress(), items);

        Assert.NotNull(order);
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Single(order.Items);
        Assert.Equal(2, order.Items[0].Quantity);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    // =========================================================================
    // TESTES NEGATIVOS
    // =========================================================================

    [Fact]
    public async Task CreateAsync_ClienteInexistente_DeveLancarExcecao()
    {
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((CustomerEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), MakeAddress(),
                new[] { (Guid.NewGuid(), 1) }));
    }

    [Fact]
    public async Task CreateAsync_ClienteInativo_DeveLancarExcecao()
    {
        var customer = MakeCustomer();
        customer.Deactivate();

        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(customer);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(customer.Id, MakeAddress(),
                new[] { (Guid.NewGuid(), 1) }));
    }

    [Fact]
    public async Task CreateAsync_ProdutoInexistente_DeveLancarExcecao()
    {
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(MakeCustomer());
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), MakeAddress(),
                new[] { (Guid.NewGuid(), 1) }));
    }

    [Fact]
    public async Task CreateAsync_ProdutoIndisponivel_DeveLancarExcecao()
    {
        var product = MakeProduct(stock: 0); // estoque 0 → IsAvailable false

        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(MakeCustomer());
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), MakeAddress(),
                new[] { (product.Id, 1) }));
    }

    [Fact]
    public void Constructor_NullDependencies_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new OrderFactory(null!, _productRepoMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new OrderFactory(_customerRepoMock.Object, null!));
    }
}
