// =============================================================================
// SOLID - SRP: responsabilidade única — definir igualdade estrutural para VOs.
// SOLID - OCP (Open/Closed Principle): aberta para extensão (herança dos VOs
// concretos), fechada para modificação desta base.
// =============================================================================

namespace Domain.Common;

/// <summary>
/// Classe base abstrata para Value Objects.
/// Value Objects são definidos pelos seus atributos, não por identidade.
/// São imutáveis por design.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// As classes derivadas devem retornar todos os componentes que participam
    /// da comparação de igualdade.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
