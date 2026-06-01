// =============================================================================
// TESTES UNITÁRIOS - Order (Aggregate Root)
// Cobre: ciclo de vida, adição/remoção de itens, transições de status,
//        eventos de domínio e casos de erro.
// TDD: testes escritos antes/junto à implementação.
// =============================================================================

using Domain.OrderManagement.Entities;
using Domain.OrderManagement.Events;
using Domain.OrderManagement.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class OrderTests
{
    // -------------------------------------------------------------------------
    // Helpers de fábrica para testes
    // -------------------------------------------------------------------------
    private static Address MakeAddress() => new(
        "Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01310-100", "Brasil");

    private static Money Price(decimal v) => Money.FromBRL(v);

    private static Order MakeOrder(Guid? customerId = null) =>
        new(Guid.NewGuid(), customerId ?? Guid.NewGuid(), MakeAddress());

    private static Guid ProductId1 = Guid.NewGuid();
    private static Guid ProductId2 = Guid.NewGuid();

    // -------------------------------------------------------------------------
    // TESTES POSITIVOS — Criação
    // -------------------------------------------------------------------------

    [Fact]
    public void Order_NewOrder_ShouldHavePendingStatus()
    {
        var order = MakeOrder();
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Order_NewOrder_ShouldHaveNoItems()
    {
        var order = MakeOrder();
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Order_NewOrder_TotalShouldBeZero()
    {
        var order = MakeOrder();
        Assert.Equal(0m, order.Total.Amount);
    }

    // -------------------------------------------------------------------------
    // TESTES POSITIVOS — Itens
    // -------------------------------------------------------------------------

    [Fact]
    public void AddItem_ValidItem_ShouldAddToItems()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));

        Assert.Single(order.Items);
        Assert.Equal(3500m, order.Total.Amount);
    }

    [Fact]
    public void AddItem_SameProduct_ShouldAccumulateQuantity()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.AddItem(ProductId1, "Notebook", 2, Price(3500m));

        Assert.Single(order.Items);
        Assert.Equal(3, order.Items[0].Quantity);
        Assert.Equal(10500m, order.Total.Amount);
    }

    [Fact]
    public void AddItem_MultipleProducts_ShouldCalculateTotalCorrectly()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.AddItem(ProductId2, "Mouse", 2, Price(150m));

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(3800m, order.Total.Amount);
    }

    [Fact]
    public void RemoveItem_ExistingProduct_ShouldRemoveFromList()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.RemoveItem(ProductId1);

        Assert.Empty(order.Items);
        Assert.Equal(0m, order.Total.Amount);
    }

    // -------------------------------------------------------------------------
    // TESTES POSITIVOS — Ciclo de vida / Status
    // -------------------------------------------------------------------------

    [Fact]
    public void Confirm_WithItems_ShouldChangeStatusToConfirmed()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_ShouldRaiseDomainEvent()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();

        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderPlacedEvent>(order.DomainEvents.First());
    }

    [Fact]
    public void Confirm_OrderPlacedEvent_ShouldHaveCorrectTotal()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 2, Price(3500m));
        order.Confirm();

        var evt = (OrderPlacedEvent)order.DomainEvents.First();
        Assert.Equal(7000m, evt.Total.Amount);
    }

    [Fact]
    public void FullLifecycle_PendingToDelivered_ShouldSucceed()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));

        order.Confirm();
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        order.MarkAsPaid();
        Assert.Equal(OrderStatus.Paid, order.Status);

        order.Ship();
        Assert.Equal(OrderStatus.Shipped, order.Status);

        order.Deliver();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Cancel_PendingOrder_ShouldRaiseCancelledEvent()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderCancelledEvent>(order.DomainEvents.First());
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();

        order.ClearDomainEvents();

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void UpdateDeliveryAddress_WhilePending_ShouldUpdate()
    {
        var order = MakeOrder();
        var newAddress = new Address("Av. Paulista", "1", null, "Bela Vista", "SP", "SP", "01310-100", "Brasil");

        order.UpdateDeliveryAddress(newAddress);

        Assert.Equal("Av. Paulista", order.DeliveryAddress.Street);
    }

    // -------------------------------------------------------------------------
    // TESTES NEGATIVOS
    // -------------------------------------------------------------------------

    [Fact]
    public void Order_EmptyCustomerId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Order(Guid.NewGuid(), Guid.Empty, MakeAddress()));
    }

    [Fact]
    public void Order_NullAddress_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Order(Guid.NewGuid(), Guid.NewGuid(), null!));
    }

    [Fact]
    public void Confirm_WithNoItems_ShouldThrow()
    {
        var order = MakeOrder();
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void AddItem_AfterConfirm_ShouldThrow()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();

        Assert.Throws<InvalidOperationException>(() =>
            order.AddItem(ProductId2, "Mouse", 1, Price(100m)));
    }

    [Fact]
    public void RemoveItem_NonExistingProduct_ShouldThrow()
    {
        var order = MakeOrder();
        Assert.Throws<InvalidOperationException>(() =>
            order.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void InvalidTransition_PendingToShipped_ShouldThrow()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();

        // Pula "Paid" — deve falhar
        Assert.Throws<InvalidOperationException>(() => order.Ship());
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldThrow()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void UpdateDeliveryAddress_AfterPayment_ShouldThrow()
    {
        var order = MakeOrder();
        order.AddItem(ProductId1, "Notebook", 1, Price(3500m));
        order.Confirm();
        order.MarkAsPaid();

        var newAddress = MakeAddress();
        Assert.Throws<InvalidOperationException>(() => order.UpdateDeliveryAddress(newAddress));
    }
}
