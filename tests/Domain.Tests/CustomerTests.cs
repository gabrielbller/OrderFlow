// =============================================================================
// TESTES UNITÁRIOS - Customer (Aggregate Root - Customer BC)
// Cobre: criação, atualização de dados, ativação/desativação.
// =============================================================================

using Domain.Customer.ValueObjects;
using Xunit;
using CustomerEntity = Domain.Customer.Entities.Customer;

namespace Domain.Tests;

public class CustomerTests
{
    private static CustomerEntity MakeCustomer() => new(
        Guid.NewGuid(),
        "Gabriel Bller",
        new Email("gabriel@email.com"),
        new Cpf("529.982.247-25"),   // CPF válido para teste
        new DateTime(1990, 1, 1));

    // -------------------------------------------------------------------------
    // TESTES POSITIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Customer_ValidData_ShouldCreate()
    {
        var c = MakeCustomer();

        Assert.Equal("Gabriel Bller", c.FullName);
        Assert.Equal("gabriel@email.com", c.Email.Value);
        Assert.True(c.IsActive);
    }

    [Fact]
    public void UpdateEmail_ValidEmail_ShouldUpdate()
    {
        var c = MakeCustomer();
        c.UpdateEmail(new Email("novo@email.com"));

        Assert.Equal("novo@email.com", c.Email.Value);
    }

    [Fact]
    public void UpdateFullName_ValidName_ShouldUpdate()
    {
        var c = MakeCustomer();
        c.UpdateFullName("Gabriel B. Souza");

        Assert.Equal("Gabriel B. Souza", c.FullName);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var c = MakeCustomer();
        c.Deactivate();
        Assert.False(c.IsActive);
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveTrue()
    {
        var c = MakeCustomer();
        c.Deactivate();
        c.Activate();
        Assert.True(c.IsActive);
    }

    // Email VO
    [Fact]
    public void Email_Valid_ShouldNormalizeLowercase()
    {
        var email = new Email("Test@EMAIL.COM");
        Assert.Equal("test@email.com", email.Value);
    }

    [Fact]
    public void Email_Equality_SameValue_ShouldBeEqual()
    {
        var a = new Email("a@b.com");
        var b = new Email("a@b.com");
        Assert.Equal(a, b);
    }

    // CPF VO
    [Fact]
    public void Cpf_Valid_ShouldStoreOnlyDigits()
    {
        var cpf = new Cpf("529.982.247-25");
        Assert.Equal("52998224725", cpf.Value);
    }

    [Fact]
    public void Cpf_Formatted_ShouldReturnCorrectFormat()
    {
        var cpf = new Cpf("529.982.247-25");
        Assert.Equal("529.982.247-25", cpf.Formatted);
    }

    // -------------------------------------------------------------------------
    // TESTES NEGATIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Customer_EmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new CustomerEntity(Guid.NewGuid(), "", new Email("a@b.com"),
                new Cpf("529.982.247-25"), new DateTime(1990, 1, 1)));
    }

    [Fact]
    public void Customer_Under18_ShouldThrow()
    {
        var birthDate = DateTime.Today.AddYears(-17);
        Assert.Throws<InvalidOperationException>(() =>
            new CustomerEntity(Guid.NewGuid(), "Nome", new Email("a@b.com"),
                new Cpf("529.982.247-25"), birthDate));
    }

    [Fact]
    public void UpdateEmail_Null_ShouldThrow()
    {
        var c = MakeCustomer();
        Assert.Throws<ArgumentNullException>(() => c.UpdateEmail(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalido")]
    [InlineData("sem@dominio")]
    public void Email_Invalid_ShouldThrow(string value)
    {
        Assert.Throws<ArgumentException>(() => new Email(value));
    }

    [Fact]
    public void Cpf_Invalid_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Cpf("111.111.111-11")); // todos iguais
    }

    [Fact]
    public void Cpf_WrongLength_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Cpf("123.456.789"));
    }

    [Fact]
    public void UpdateFullName_EmptyName_ShouldThrow()
    {
        var c = MakeCustomer();
        Assert.Throws<ArgumentException>(() => c.UpdateFullName(""));
    }
}
