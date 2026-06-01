// =============================================================================
// DDD - Value Object: ProductCode (Bounded Context: Inventory)
// SOLID - SRP: representa e valida o código único de um produto.
// OO - Encapsulamento: valor imutável com construtor validando formato.
// =============================================================================

using Domain.Common;

namespace Domain.Inventory.ValueObjects;

/// <summary>
/// Value Object que representa o código único de um produto (SKU).
/// Formato: letras maiúsculas e números, 4–12 caracteres.
/// </summary>
public sealed class ProductCode : ValueObject
{
    public string Value { get; }

    public ProductCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Código do produto não pode ser vazio.");

        value = value.Trim().ToUpperInvariant();

        if (value.Length < 4 || value.Length > 12)
            throw new ArgumentException("Código deve ter entre 4 e 12 caracteres.");

        if (!value.All(c => char.IsLetterOrDigit(c) || c == '-'))
            throw new ArgumentException("Código deve conter apenas letras, dígitos ou hífen.");

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
