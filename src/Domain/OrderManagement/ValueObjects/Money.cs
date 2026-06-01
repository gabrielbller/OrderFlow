// =============================================================================
// DDD - Value Object: Money
// Representa um valor monetário com moeda. Imutável por design.
// SOLID - SRP: única responsabilidade — encapsular um valor monetário e suas
//   operações aritméticas válidas.
// GRASP - High Cohesion: todos os métodos são diretamente relacionados a dinheiro.
// =============================================================================

using Domain.Common;

namespace Domain.OrderManagement.ValueObjects;

/// <summary>
/// Value Object que representa um valor monetário.
/// Imutável: cada operação retorna uma nova instância.
/// </summary>
public sealed class Money : ValueObject
{
    // OO - Encapsulamento: setters privados impedem mutação externa
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Moeda deve ser um código ISO 4217 de 3 letras.", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    // Factory method — conveniente para BRL
    public static Money FromBRL(decimal amount) => new(amount, "BRL");

    public static Money Zero(string currency) => new(0, currency);

    // SOLID - OCP: novos operadores podem ser adicionados sem modificar os existentes
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Resultado de subtração não pode ser negativo.");
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Fator de multiplicação não pode ser negativo.");
        return new Money(Amount * factor, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Operação entre moedas diferentes: {Currency} e {other.Currency}.");
    }

    // OO - Polimorfismo: sobrescrita do método da classe base ValueObject
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Currency} {Amount:F2}");
}
