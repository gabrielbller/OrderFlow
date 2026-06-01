// =============================================================================
// DDD - Aggregate Root
// O AggregateRoot é a porta de entrada para o Aggregate. Toda mutação de
// estado deve passar por ele. Ele também é responsável por publicar eventos
// de domínio.
// GRASP - Low Coupling: o AggregateRoot desacopla as entidades internas
// do mundo externo, expondo apenas o que é necessário.
// =============================================================================

namespace Domain.Common;

/// <summary>
/// Classe base para Aggregate Roots.
/// Herda de Entity (identidade) e gerencia os Domain Events.
/// </summary>
public abstract class AggregateRoot : Entity
{
    // Lista privada de eventos — encapsulamento total (OO - Encapsulamento)
    private readonly List<DomainEvent> _domainEvents = new();

    // Exposição somente-leitura dos eventos (interface clara e concisa)
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>
    /// Registra um evento de domínio para ser despachado após persistência.
    /// </summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpa os eventos após terem sido processados.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
