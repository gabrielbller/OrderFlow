// =============================================================================
// GRASP - Controller
// O padrão GRASP Controller define que deve existir um objeto responsável por
// RECEBER e COORDENAR as operações do sistema, sem executá-las diretamente.
// O Controller não contém lógica de domínio — ele delega para Domain Services,
// Factories e Repositories.
//
// Objetivo do padrão Controller:
//   Separar a camada de entrada (API, CLI, gRPC) da lógica de negócio.
//   O Controller conhece o "quê" deve ser feito, mas não o "como".
//
// SOLID - SRP: OrderApplicationService coordena apenas fluxos de pedido.
// SOLID - DIP: depende exclusivamente de abstrações (interfaces).
// GRASP - Low Coupling: não acessa repositórios de produto/cliente diretamente.
// GRASP - High Cohesion: todos os métodos são casos de uso de Pedido.
// =============================================================================

using Domain.Customer.Repositories;
using Domain.Inventory.Repositories;
using Domain.OrderManagement.Entities;
using Domain.OrderManagement.Factories;
using Domain.OrderManagement.Repositories;
using Domain.OrderManagement.Services;
using Domain.OrderManagement.ValueObjects;

namespace Application.Orders;

/// <summary>
/// GRASP Controller — coordena casos de uso relacionados a pedidos.
/// Recebe comandos da camada de apresentação e orquestra as chamadas
/// ao domínio sem conter regras de negócio.
/// </summary>
public class OrderApplicationService
{
    // SOLID - DIP: todas as dependências são injetadas via construtor (abstrações)
    private readonly OrderFactory       _orderFactory;
    private readonly OrderDomainService _orderDomainService;
    private readonly IOrderRepository   _orderRepository;

    /// <summary>
    /// Construtor com injeção de dependência — garante que as dependências
    /// sejam fornecidas em tempo de construção (fail-fast).
    /// </summary>
    public OrderApplicationService(
        OrderFactory orderFactory,
        OrderDomainService orderDomainService,
        IOrderRepository orderRepository)
    {
        _orderFactory       = orderFactory       ?? throw new ArgumentNullException(nameof(orderFactory));
        _orderDomainService = orderDomainService ?? throw new ArgumentNullException(nameof(orderDomainService));
        _orderRepository    = orderRepository    ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    // =========================================================================
    // Caso de Uso 1: Criar e submeter um novo pedido
    // =========================================================================

    /// <summary>
    /// Cria um novo pedido para o cliente com os itens fornecidos.
    /// GRASP Controller: recebe o comando e delega à Factory de domínio.
    /// </summary>
    public async Task<Guid> PlaceOrderAsync(
        Guid customerId,
        Address deliveryAddress,
        IEnumerable<(Guid ProductId, int Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        // Delega criação à Factory (DDD) — não instancia Order diretamente
        var order = await _orderFactory.CreateAsync(
            customerId, deliveryAddress, items, cancellationToken);

        // Delega confirmação ao Domain Service — não executa regras aqui
        await _orderDomainService.ConfirmOrderAsync(order, cancellationToken);

        await _orderRepository.AddAsync(order, cancellationToken);
        return order.Id;
    }

    // =========================================================================
    // Caso de Uso 2: Processar pagamento de um pedido
    // =========================================================================

    /// <summary>
    /// Processa o pagamento de um pedido existente.
    /// GRASP Controller: coordena sem executar lógica de cobrança.
    /// </summary>
    public async Task ProcessPaymentAsync(
        Guid orderId,
        string paymentToken,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderOrThrowAsync(orderId, cancellationToken);

        // Delega ao Domain Service — ACL cuida da comunicação com gateway
        await _orderDomainService.ProcessPaymentAsync(order, paymentToken, cancellationToken);
    }

    // =========================================================================
    // Caso de Uso 3: Cancelar pedido
    // =========================================================================

    /// <summary>
    /// Cancela um pedido existente.
    /// </summary>
    public async Task CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        await _orderDomainService.CancelOrderAsync(order, cancellationToken);
    }

    // =========================================================================
    // Caso de Uso 4: Consultar pedido
    // =========================================================================

    /// <summary>
    /// Retorna um pedido pelo seu identificador.
    /// </summary>
    public async Task<Order> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await GetOrderOrThrowAsync(orderId, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Método auxiliar privado — encapsulamento de consulta + verificação
    // -------------------------------------------------------------------------
    private async Task<Order> GetOrderOrThrowAsync(
        Guid orderId, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Pedido {orderId} não encontrado.");
    }
}
