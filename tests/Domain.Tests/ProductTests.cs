// =============================================================================
// TESTES UNITÁRIOS - Product (Aggregate Root - Inventory BC)
// Cobre: criação, reserva de estoque, reposição, ativação/desativação.
// =============================================================================

using Domain.Inventory.Entities;
using Domain.Inventory.ValueObjects;
using Domain.OrderManagement.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class ProductTests
{
    private static Product MakeProduct(int stock = 10) =>
        new(Guid.NewGuid(), new ProductCode("NTB-001"), "Notebook", "Notebook básico",
            Money.FromBRL(3500m), stock);

    // -------------------------------------------------------------------------
    // TESTES POSITIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Product_WithStock_ShouldBeAvailable()
    {
        var p = MakeProduct(5);
        Assert.True(p.IsAvailable);
        Assert.Equal(5, p.StockQuantity);
    }

    [Fact]
    public void ReserveStock_ValidQuantity_ShouldDecreaseStock()
    {
        var p = MakeProduct(10);
        p.ReserveStock(3);

        Assert.Equal(7, p.StockQuantity);
        Assert.True(p.IsAvailable);
    }

    [Fact]
    public void ReserveStock_AllUnits_ShouldBecomeUnavailable()
    {
        var p = MakeProduct(5);
        p.ReserveStock(5);

        Assert.Equal(0, p.StockQuantity);
        Assert.False(p.IsAvailable);
    }

    [Fact]
    public void ReplenishStock_ShouldIncreaseAndBecomeAvailable()
    {
        var p = MakeProduct(0);
        p.ReplenishStock(10);

        Assert.Equal(10, p.StockQuantity);
        Assert.True(p.IsAvailable);
    }

    [Fact]
    public void UpdatePrice_ValidPrice_ShouldUpdate()
    {
        var p = MakeProduct();
        var newPrice = Money.FromBRL(4000m);
        p.UpdatePrice(newPrice);

        Assert.Equal(4000m, p.Price.Amount);
    }

    [Fact]
    public void Deactivate_ShouldSetIsAvailableFalse()
    {
        var p = MakeProduct(10);
        p.Deactivate();

        Assert.False(p.IsAvailable);
        // estoque não deve ser zerado pela desativação
        Assert.Equal(10, p.StockQuantity);
    }

    [Fact]
    public void ProductCode_Valid_ShouldCreate()
    {
        var code = new ProductCode("SKU-001");
        Assert.Equal("SKU-001", code.Value);
    }

    // -------------------------------------------------------------------------
    // TESTES NEGATIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Product_NegativeStock_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(Guid.NewGuid(), new ProductCode("NTB-001"),
                "Notebook", "", Money.FromBRL(100m), -1));
    }

    [Fact]
    public void Product_EmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(Guid.NewGuid(), new ProductCode("NTB-001"),
                "", "", Money.FromBRL(100m), 10));
    }

    [Fact]
    public void ReserveStock_MoreThanAvailable_ShouldThrow()
    {
        var p = MakeProduct(5);
        Assert.Throws<InvalidOperationException>(() => p.ReserveStock(10));
    }

    [Fact]
    public void ReserveStock_ZeroQuantity_ShouldThrow()
    {
        var p = MakeProduct(10);
        Assert.Throws<ArgumentException>(() => p.ReserveStock(0));
    }

    [Fact]
    public void ReplenishStock_ZeroQuantity_ShouldThrow()
    {
        var p = MakeProduct();
        Assert.Throws<ArgumentException>(() => p.ReplenishStock(0));
    }

    [Fact]
    public void ProductCode_TooShort_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new ProductCode("AB"));
    }

    [Fact]
    public void ProductCode_TooLong_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new ProductCode("ABCDE12345678"));
    }

    [Fact]
    public void UpdatePrice_Null_ShouldThrow()
    {
        var p = MakeProduct();
        Assert.Throws<ArgumentNullException>(() => p.UpdatePrice(null!));
    }
}
