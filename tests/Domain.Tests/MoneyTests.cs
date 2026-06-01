// =============================================================================
// TESTES UNITÁRIOS - Money (Value Object)
// Princípios F.I.R.S.T:
//   Fast: sem I/O, sem rede
//   Isolated: cada teste é independente
//   Repeatable: determinístico (sem randomness)
//   Self-verifying: usa Assert
//   Timely: escrito junto ao código de produção (TDD)
// =============================================================================

using Domain.OrderManagement.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class MoneyTests
{
    // -------------------------------------------------------------------------
    // TESTES POSITIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_ValidAmount_ShouldCreateMoney()
    {
        var money = new Money(100.50m, "BRL");

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("BRL", money.Currency);
    }

    [Fact]
    public void Constructor_ShouldNormalizeToUpperCase()
    {
        var money = new Money(10m, "brl");
        Assert.Equal("BRL", money.Currency);
    }

    [Fact]
    public void Constructor_ShouldRoundToTwoDecimals()
    {
        var money = new Money(10.555m, "BRL");
        Assert.Equal(10.56m, money.Amount);
    }

    [Fact]
    public void FromBRL_ShouldCreateBRLMoney()
    {
        var money = Money.FromBRL(250m);
        Assert.Equal("BRL", money.Currency);
        Assert.Equal(250m, money.Amount);
    }

    [Fact]
    public void Zero_ShouldCreateZeroAmount()
    {
        var money = Money.Zero("BRL");
        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Add_SameCurrency_ShouldSumAmounts()
    {
        var a = Money.FromBRL(100m);
        var b = Money.FromBRL(50m);

        var result = a.Add(b);

        Assert.Equal(150m, result.Amount);
        Assert.Equal("BRL", result.Currency);
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldSubtractAmounts()
    {
        var a = Money.FromBRL(100m);
        var b = Money.FromBRL(30m);

        var result = a.Subtract(b);

        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Multiply_ValidFactor_ShouldMultiplyAmount()
    {
        var money = Money.FromBRL(50m);
        var result = money.Multiply(3);

        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ShouldBeEqual()
    {
        var a = new Money(100m, "BRL");
        var b = new Money(100m, "BRL");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equals_DifferentAmounts_ShouldNotBeEqual()
    {
        var a = Money.FromBRL(100m);
        var b = Money.FromBRL(200m);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void IsGreaterThan_ShouldReturnCorrectResult()
    {
        var bigger = Money.FromBRL(200m);
        var smaller = Money.FromBRL(100m);

        Assert.True(bigger.IsGreaterThan(smaller));
        Assert.False(smaller.IsGreaterThan(bigger));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var money = new Money(99.90m, "BRL");
        Assert.Equal("BRL 99.90", money.ToString());
    }

    // -------------------------------------------------------------------------
    // TESTES NEGATIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_NegativeAmount_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Money(-1m, "BRL"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("BR")]       // menos de 3
    [InlineData("BRLS")]     // mais de 3
    public void Constructor_InvalidCurrency_ShouldThrow(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, currency));
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrow()
    {
        var brl = Money.FromBRL(100m);
        var usd = new Money(100m, "USD");

        Assert.Throws<InvalidOperationException>(() => brl.Add(usd));
    }

    [Fact]
    public void Subtract_ResultNegative_ShouldThrow()
    {
        var a = Money.FromBRL(10m);
        var b = Money.FromBRL(50m);

        Assert.Throws<InvalidOperationException>(() => a.Subtract(b));
    }

    [Fact]
    public void Multiply_NegativeFactor_ShouldThrow()
    {
        var money = Money.FromBRL(50m);
        Assert.Throws<ArgumentException>(() => money.Multiply(-1));
    }

    [Fact]
    public void IsGreaterThan_DifferentCurrencies_ShouldThrow()
    {
        var brl = Money.FromBRL(200m);
        var usd = new Money(100m, "USD");

        Assert.Throws<InvalidOperationException>(() => brl.IsGreaterThan(usd));
    }
}
