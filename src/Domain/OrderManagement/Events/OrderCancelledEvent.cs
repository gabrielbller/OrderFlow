using Domain.Common;

namespace Domain.OrderManagement.Events;

/// <summary>
/// Evento de domínio disparado quando um pedido é cancelado.
/// </summary>
public sealed class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }

    public OrderCancelledEvent(Guid orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}
