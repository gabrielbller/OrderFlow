// =============================================================================
// DDD - Value Object: Email (Bounded Context: Customer)
// OO - Encapsulamento: construtor valida e normaliza o valor.
// SOLID - SRP: única responsabilidade — representar e validar um e-mail.
// =============================================================================

using Domain.Common;
using System.Text.RegularExpressions;

namespace Domain.Customer.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de e-mail válido.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("E-mail não pode ser vazio.");

        value = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"'{value}' não é um e-mail válido.");

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
