namespace Domain.Common;

/// <summary>
/// Classe base para eventos de domínio.
/// Um evento de domínio representa algo relevante que aconteceu no domínio.
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
