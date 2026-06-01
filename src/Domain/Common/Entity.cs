// =============================================================================
// SOLID - SRP (Single Responsibility Principle)
// Esta classe tem UMA única responsabilidade: ser a base de identidade para
// todas as Entidades do domínio. Ela não contém regras de negócio.
// =============================================================================

namespace Domain.Common;

/// <summary>
/// Classe base abstrata para todas as Entidades do domínio.
/// Uma Entidade é definida pela sua identidade (Id), não pelos seus atributos.
/// </summary>
public abstract class Entity
{
    // GRASP - High Cohesion: apenas atributos e comportamentos relacionados
    // à identidade ficam aqui.
    public Guid Id { get; protected set; }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id da entidade não pode ser vazio.", nameof(id));

        Id = id;
    }

    // Igualdade baseada em identidade (não em valores de atributos)
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);

    public override int GetHashCode() => Id.GetHashCode();
}
