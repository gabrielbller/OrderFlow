// =============================================================================
// TESTES UNITÁRIOS - OrderStatus (Value Object)
// Cobre todas as transições válidas e inválidas do ciclo de vida do pedido.
// =============================================================================

using Domain.OrderManagement.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class OrderStatusTests
{
    [Theory]
    [InlineData("Pending",   "Confirmed", true)]
    [InlineData("Pending",   "Cancelled", true)]
    [InlineData("Confirmed", "Paid",      true)]
    [InlineData("Confirmed", "Cancelled", true)]
    [InlineData("Paid",      "Shipped",   true)]
    [InlineData("Paid",      "Cancelled", true)]
    [InlineData("Shipped",   "Delivered", true)]
    public void CanTransitionTo_ValidTransitions_ShouldReturnTrue(
        string from, string to, bool expected)
    {
        var fromStatus = GetStatus(from);
        var toStatus   = GetStatus(to);

        Assert.Equal(expected, fromStatus.CanTransitionTo(toStatus));
    }

    [Theory]
    [InlineData("Pending",   "Paid")]
    [InlineData("Pending",   "Shipped")]
    [InlineData("Confirmed", "Shipped")]
    [InlineData("Shipped",   "Cancelled")]
    [InlineData("Delivered", "Cancelled")]
    [InlineData("Cancelled", "Pending")]
    public void CanTransitionTo_InvalidTransitions_ShouldReturnFalse(string from, string to)
    {
        var fromStatus = GetStatus(from);
        var toStatus   = GetStatus(to);

        Assert.False(fromStatus.CanTransitionTo(toStatus));
    }

    [Fact]
    public void Equality_SameStatus_ShouldBeEqual()
    {
        var pending = OrderStatus.Pending;
        Assert.True(pending == OrderStatus.Pending);
        Assert.False(pending == OrderStatus.Confirmed);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        Assert.Equal("Pending", OrderStatus.Pending.ToString());
        Assert.Equal("Cancelled", OrderStatus.Cancelled.ToString());
    }

    private static OrderStatus GetStatus(string name) => name switch
    {
        "Pending"   => OrderStatus.Pending,
        "Confirmed" => OrderStatus.Confirmed,
        "Paid"      => OrderStatus.Paid,
        "Shipped"   => OrderStatus.Shipped,
        "Delivered" => OrderStatus.Delivered,
        "Cancelled" => OrderStatus.Cancelled,
        _ => throw new ArgumentException($"Status desconhecido: {name}")
    };
}
