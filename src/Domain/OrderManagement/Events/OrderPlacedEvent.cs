using Domain.Common;
using Domain.OrderManagement.ValueObjects;

namespace Domain.OrderManagement.Events;

/// <summary>
/// Evento de domínio disparado quando um pedido é confirmado.
/// </summary>
public sealed class OrderPlacedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public Money Total { get; }

    public OrderPlacedEvent(Guid orderId, Guid customerId, Money total)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
    }
}
