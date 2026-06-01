// =============================================================================
// DDD - Entity dentro do Aggregate Order
// OrderItem existe apenas dentro do Aggregate Order; nunca é acessado
// diretamente via repositório.
// OO - Encapsulamento: mutação de quantidade controlada pela própria classe.
// SOLID - SRP: representa um único item do pedido e seus cálculos de subtotal.
// =============================================================================

using Domain.Common;
using Domain.OrderManagement.ValueObjects;

namespace Domain.OrderManagement.Entities;

/// <summary>
/// Entidade que representa um item dentro de um pedido.
/// Faz parte do Aggregate cujo root é <see cref="Order"/>.
/// </summary>
public sealed class OrderItem : Entity
{
    // OO - Encapsulamento: somente leitura externamente
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; }

    // OO - Abstração: o subtotal é calculado, não armazenado redundantemente
    public Money Subtotal => UnitPrice.Multiply(Quantity);

    // OO - Construtor com validação (contrato claro)
    internal OrderItem(Guid id, Guid productId, string productName,
                       int quantity, Money unitPrice) : base(id)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId inválido.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Nome do produto é obrigatório.");
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        if (unitPrice is null)
            throw new ArgumentNullException(nameof(unitPrice));

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Atualiza a quantidade do item. Só pode ser chamado via Order (root).
    /// </summary>
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        Quantity = newQuantity;
    }
}
