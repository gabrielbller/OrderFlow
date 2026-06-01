// =============================================================================
// DDD - Anti-Corruption Layer (ACL) — Fronteira de domínio
// Esta interface pertence ao DOMÍNIO e define o contrato na linguagem do
// domínio (Money, PaymentResult). A implementação concreta (adapter) fica
// na infraestrutura e traduz para/do gateway externo.
//
// SOLID - DIP: o domínio depende desta abstração; a infraestrutura implementa.
// SOLID - ISP: interface mínima e específica para pagamentos.
// GRASP - Low Coupling: o domínio nunca toca a API do gateway externo.
// =============================================================================

using Domain.OrderManagement.ValueObjects;
using Domain.Payment.ValueObjects;

namespace Domain.Payment.Services;

/// <summary>
/// Interface do serviço de pagamento — fronteira da Anti-Corruption Layer.
/// O domínio conhece apenas esta interface; o adapter externo implementa.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Realiza a cobrança para um pedido.
    /// </summary>
    Task<PaymentResult> ChargeAsync(
        Guid orderId,
        Money amount,
        string paymentToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solicita o estorno de uma cobrança.
    /// </summary>
    Task<PaymentResult> RefundAsync(
        string transactionId,
        Money amount,
        CancellationToken cancellationToken = default);
}
