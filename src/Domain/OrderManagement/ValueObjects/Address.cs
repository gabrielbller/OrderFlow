// =============================================================================
// DDD - Value Object: Address
// Endereço de entrega do pedido. Imutável e comparado por valor.
// SOLID - SRP: única responsabilidade — representar e validar um endereço.
// OO - Encapsulamento: todos os setters são privados (init-only).
// =============================================================================

using Domain.Common;

namespace Domain.OrderManagement.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de entrega.
/// Comparado por igualdade estrutural (todos os campos).
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string number, string? complement,
                   string neighborhood, string city, string state,
                   string zipCode, string country)
    {
        Street = Guard(street, nameof(street));
        Number = Guard(number, nameof(number));
        Complement = complement;
        Neighborhood = Guard(neighborhood, nameof(neighborhood));
        City = Guard(city, nameof(city));
        State = Guard(state, nameof(state));
        ZipCode = Guard(zipCode, nameof(zipCode));
        Country = Guard(country, nameof(country));
    }

    private static string Guard(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} não pode ser vazio.");
        return value.Trim();
    }

    // OO - Polimorfismo: sobrescrita do método abstrato da classe base
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement ?? string.Empty;
        yield return Neighborhood;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() =>
        $"{Street}, {Number}{(Complement != null ? $", {Complement}" : "")} — {Neighborhood}, {City}/{State} — {ZipCode}";
}
