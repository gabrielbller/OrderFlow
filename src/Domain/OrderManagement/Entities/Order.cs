// =============================================================================
// DDD - Aggregate Root: Order
// Order é o ponto central do Bounded Context de Gestão de Pedidos.
// Toda mutação nos itens do pedido passa obrigatoriamente por esta classe.
//
// SOLID - SRP: Order gerencia o ciclo de vida do pedido e seus itens.
//   Regras de pricing e estoque ficam em Domain Services separados.
// SOLID - OCP: novos comportamentos são adicionados sem modificar esta classe
//   (ex: novos eventos de domínio, novas transições via OrderStatus).
// GRASP - Low Coupling: Order depende apenas de abstrações (IDs de produto,
//   Money VO) e não de classes concretas de outros BCs.
// GRASP - High Cohesion: todos os métodos desta classe são sobre o ciclo
//   de vida de um Pedido.
// =============================================================================

using Domain.Common;
using Domain.OrderManagement.Events;
using Domain.OrderManagement.ValueObjects;

namespace Domain.OrderManagement.Entities;

/// <summary>
/// Aggregate Root do Bounded Context de Gestão de Pedidos.
/// Encapsula todas as regras de negócio relativas a um pedido.
/// </summary>
public sealed class Order : AggregateRoot
{
    // OO - Encapsulamento: lista interna não exposta diretamente
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; }
    public Address DeliveryAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    // OO - Abstração: Total calculado a partir dos itens — sem redundância
    public Money Total => _items
        .Aggregate(Money.Zero("BRL"), (acc, item) => acc.Add(item.Subtotal));

    // Leitura somente (IReadOnlyList) — encapsulamento de coleção
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // Construtor interno — instanciação via OrderFactory (DDD pattern)
    internal Order(Guid id, Guid customerId, Address deliveryAddress) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId inválido.", nameof(customerId));
        if (deliveryAddress is null)
            throw new ArgumentNullException(nameof(deliveryAddress));

        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // ==========================================================================
    // Regras de negócio — métodos de domínio
    // ==========================================================================

    /// <summary>
    /// Adiciona um item ao pedido. Apenas em status Pending.
    /// </summary>
    public void AddItem(Guid productId, string productName, int quantity, Money unitPrice)
    {
        EnsureStatus(OrderStatus.Pending, "adicionar itens");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            // Se produto já existe, incrementa a quantidade
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(Guid.NewGuid(), productId, productName, quantity, unitPrice));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove um item do pedido pelo ID do produto.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        EnsureStatus(OrderStatus.Pending, "remover itens");

        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"Item com produto {productId} não encontrado.");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirma o pedido — transição Pending → Confirmed.
    /// </summary>
    public void Confirm()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Pedido sem itens não pode ser confirmado.");

        TransitionTo(OrderStatus.Confirmed);
        AddDomainEvent(new OrderPlacedEvent(Id, CustomerId, Total));
    }

    /// <summary>
    /// Marca o pedido como pago — transição Confirmed → Paid.
    /// </summary>
    public void MarkAsPaid()
    {
        TransitionTo(OrderStatus.Paid);
    }

    /// <summary>
    /// Marca o pedido como enviado — transição Paid → Shipped.
    /// </summary>
    public void Ship()
    {
        TransitionTo(OrderStatus.Shipped);
    }

    /// <summary>
    /// Marca o pedido como entregue — transição Shipped → Delivered.
    /// </summary>
    public void Deliver()
    {
        TransitionTo(OrderStatus.Delivered);
    }

    /// <summary>
    /// Cancela o pedido — transição Pending ou Confirmed → Cancelled.
    /// </summary>
    public void Cancel()
    {
        TransitionTo(OrderStatus.Cancelled);
        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId));
    }

    /// <summary>
    /// Atualiza o endereço de entrega. Apenas em status Pending ou Confirmed.
    /// </summary>
    public void UpdateDeliveryAddress(Address newAddress)
    {
        if (Status == OrderStatus.Paid || Status == OrderStatus.Shipped
            || Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Não é possível alterar endereço neste status.");

        DeliveryAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        UpdatedAt = DateTime.UtcNow;
    }

    // ==========================================================================
    // Método auxiliar privado — encapsulamento da lógica de transição
    // OO - Encapsulamento: lógica de estado protegida de acesso externo
    // ==========================================================================
    private void TransitionTo(OrderStatus newStatus)
    {
        if (!Status.CanTransitionTo(newStatus))
            throw new InvalidOperationException(
                $"Transição de status inválida: {Status} → {newStatus}.");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Não é possível {action} com status '{Status}'.");
    }
}
