// =============================================================================
// TESTES UNITÁRIOS - Address (Value Object)
// =============================================================================

using Domain.OrderManagement.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class AddressTests
{
    private static Address MakeAddress(string street = "Rua A") =>
        new(street, "100", null, "Centro", "São Paulo", "SP", "01310-100", "Brasil");

    [Fact]
    public void Address_ValidData_ShouldCreate()
    {
        var addr = MakeAddress();
        Assert.Equal("Rua A", addr.Street);
        Assert.Equal("SP", addr.State);
    }

    [Fact]
    public void Address_Equality_SameValues_ShouldBeEqual()
    {
        var a = MakeAddress();
        var b = MakeAddress();
        Assert.Equal(a, b);
    }

    [Fact]
    public void Address_Equality_DifferentStreet_ShouldNotBeEqual()
    {
        var a = MakeAddress("Rua A");
        var b = MakeAddress("Rua B");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Address_WithComplement_ShouldIncludeInString()
    {
        var addr = new Address("Rua A", "1", "Apto 42", "Bairro", "Cidade", "SP", "00000-000", "Brasil");
        Assert.Contains("Apto 42", addr.ToString());
    }

    [Theory]
    [InlineData("", "100", "Centro", "SP", "SP", "01310-100", "Brasil")]
    [InlineData("Rua A", "", "Centro", "SP", "SP", "01310-100", "Brasil")]
    [InlineData("Rua A", "100", "", "SP", "SP", "01310-100", "Brasil")]
    public void Address_EmptyRequiredField_ShouldThrow(
        string street, string number, string neighborhood, string city, string state, string zip, string country)
    {
        Assert.Throws<ArgumentException>(() =>
            new Address(street, number, null, neighborhood, city, state, zip, country));
    }
}
