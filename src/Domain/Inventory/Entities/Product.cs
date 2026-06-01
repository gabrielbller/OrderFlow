// =============================================================================
// DDD - Aggregate Root: Product (Bounded Context: Inventory)
// OO - Herança: Product herda de AggregateRoot (que herda de Entity).
//   Demonstra hierarquia: AggregateRoot → Entity → Product
// OO - Encapsulamento: estoque só pode ser modificado via métodos do domínio.
// SOLID - SRP: Product gerencia seu ciclo de vida e estoque.
// =============================================================================

using Domain.Common;
using Domain.Inventory.ValueObjects;
using Domain.OrderManagement.ValueObjects;

namespace Domain.Inventory.Entities;

/// <summary>
/// Aggregate Root do Bounded Context de Inventário.
/// Gerencia o ciclo de vida de um produto e seu estoque.
/// </summary>
public sealed class Product : AggregateRoot
{
    // OO - Encapsulamento: campos de estado com setters privados
    public ProductCode Code { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsAvailable { get; private set; }

    public Product(Guid id, ProductCode code, string name,
                   string description, Money price, int initialStock) : base(id)
    {
        if (code is null) throw new ArgumentNullException(nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome é obrigatório.");
        if (price is null) throw new ArgumentNullException(nameof(price));
        if (initialStock < 0) throw new ArgumentException("Estoque não pode ser negativo.");

        Code = code;
        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        StockQuantity = initialStock;
        IsAvailable = initialStock > 0;
    }

    /// <summary>
    /// Reserva unidades do estoque para um pedido.
    /// </summary>
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantidade de reserva deve ser positiva.");
        if (quantity > StockQuantity)
            throw new InvalidOperationException(
                $"Estoque insuficiente. Disponível: {StockQuantity}, solicitado: {quantity}.");

        StockQuantity -= quantity;
        IsAvailable = StockQuantity > 0;
    }

    /// <summary>
    /// Repõe o estoque (liberação de reserva ou reposição).
    /// </summary>
    public void ReplenishStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantidade de reposição deve ser positiva.");

        StockQuantity += quantity;
        IsAvailable = true;
    }

    /// <summary>
    /// Atualiza o preço do produto.
    /// </summary>
    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
    }

    /// <summary>
    /// Desativa o produto (não disponível para venda).
    /// </summary>
    public void Deactivate()
    {
        IsAvailable = false;
    }
}
