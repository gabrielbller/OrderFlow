// =============================================================================
// DDD - Value Object: CPF (Bounded Context: Customer)
// Valida e armazena um CPF brasileiro de forma imutável.
// SOLID - SRP: única responsabilidade — validação de CPF.
// OO - Encapsulamento: algoritmo de validação encapsulado no construtor.
// =============================================================================

using Domain.Common;

namespace Domain.Customer.ValueObjects;

/// <summary>
/// Value Object que representa um CPF brasileiro válido.
/// Armazena apenas os dígitos (11 caracteres).
/// </summary>
public sealed class Cpf : ValueObject
{
    public string Value { get; }

    public Cpf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CPF não pode ser vazio.");

        // Remove formatação
        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new ArgumentException("CPF deve ter 11 dígitos.");

        if (!IsValid(digits))
            throw new ArgumentException($"CPF '{value}' é inválido.");

        Value = digits;
    }

    // OO - Encapsulamento: algoritmo privado de validação de CPF
    private static bool IsValid(string cpf)
    {
        // Rejeita sequências iguais (ex: 111.111.111-11)
        if (cpf.Distinct().Count() == 1) return false;

        int sum = 0;
        for (int i = 0; i < 9; i++) sum += int.Parse(cpf[i].ToString()) * (10 - i);
        int remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11) remainder = 0;
        if (remainder != int.Parse(cpf[9].ToString())) return false;

        sum = 0;
        for (int i = 0; i < 10; i++) sum += int.Parse(cpf[i].ToString()) * (11 - i);
        remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11) remainder = 0;
        return remainder == int.Parse(cpf[10].ToString());
    }

    public string Formatted =>
        $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
