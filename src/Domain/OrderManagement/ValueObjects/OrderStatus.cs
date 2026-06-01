// =============================================================================
// DDD - Value Object: OrderStatus
// Representa o ciclo de vida de um pedido usando padrão State via VO.
// SOLID - OCP: novos estados podem ser adicionados sem alterar a lógica de
//   transição das entidades que já existem.
// =============================================================================

namespace Domain.OrderManagement.ValueObjects;

/// <summary>
/// Value Object que representa o status do pedido.
/// Encapsula as regras de transição de estado válidas.
/// </summary>
public sealed class OrderStatus
{
    // OO - Encapsulamento: construtor privado — instâncias apenas via factory members
    public string Value { get; }

    private OrderStatus(string value) => Value = value;

    // Instâncias estáticas representando estados válidos (fluent, legível)
    public static readonly OrderStatus Pending    = new("Pending");
    public static readonly OrderStatus Confirmed  = new("Confirmed");
    public static readonly OrderStatus Paid       = new("Paid");
    public static readonly OrderStatus Shipped    = new("Shipped");
    public static readonly OrderStatus Delivered  = new("Delivered");
    public static readonly OrderStatus Cancelled  = new("Cancelled");

    // ==========================================================================
    // GRASP - Low Coupling: as regras de transição estão DENTRO do VO,
    // não espalhadas por outras classes. Reduz o acoplamento da Order.
    // ==========================================================================
    public bool CanTransitionTo(OrderStatus next)
    {
        return (this, next) switch
        {
            var (c, n) when c == Pending   && n == Confirmed  => true,
            var (c, n) when c == Pending   && n == Cancelled  => true,
            var (c, n) when c == Confirmed && n == Paid       => true,
            var (c, n) when c == Confirmed && n == Cancelled  => true,
            var (c, n) when c == Paid      && n == Shipped    => true,
            var (c, n) when c == Paid      && n == Cancelled  => true,  // permite estorno + liberação de estoque
            var (c, n) when c == Shipped   && n == Delivered  => true,
            _ => false
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is OrderStatus other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(OrderStatus? a, OrderStatus? b) =>
        a?.Value == b?.Value;

    public static bool operator !=(OrderStatus? a, OrderStatus? b) => !(a == b);
}
