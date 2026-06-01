// =============================================================================
// INFRAESTRUTURA - Anti-Corruption Layer: PaymentServiceAdapter
// Implementa IPaymentService (domínio) traduzindo para/do gateway externo.
//
// PADRÃO ACL: isola o domínio de mudanças na API do gateway externo.
//   Se o gateway mudar (ex: Stripe → PagSeguro), apenas este adapter muda.
//   O domínio permanece inalterado.
//
// SOLID - SRP: única responsabilidade — traduzir entre o domínio e o gateway.
// SOLID - OCP: o domínio está fechado para modificação mesmo que o gateway mude.
// SOLID - DIP: o domínio depende de IPaymentService (abstração), não deste adapter.
// GRASP - Low Coupling: domínio não conhece ExternalChargeRequest/Response.
// =============================================================================

using Domain.OrderManagement.ValueObjects;
using Domain.Payment.Services;
using Domain.Payment.ValueObjects;

namespace Infrastructure.AntiCorruptionLayer;

/// <summary>
/// Adapter da Anti-Corruption Layer: traduz chamadas do domínio para o
/// gateway externo de pagamento e converte as respostas de volta.
/// </summary>
public class PaymentServiceAdapter : IPaymentService
{
    // Em produção, seria um HttpClient para a API do gateway
    private readonly HttpClient _httpClient;

    public PaymentServiceAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Traduz a cobrança do domínio para a estrutura do gateway externo.
    /// </summary>
    public async Task<PaymentResult> ChargeAsync(
        Guid orderId, Money amount, string paymentToken,
        CancellationToken cancellationToken = default)
    {
        // TRADUÇÃO: linguagem do domínio → linguagem do gateway externo
        var request = new ExternalChargeRequest
        {
            OrderReference = orderId.ToString(),
            AmountCents    = (long)(amount.Amount * 100),  // gateway usa centavos
            Currency       = amount.Currency,
            Token          = paymentToken
        };

        // Simulação da chamada HTTP ao gateway (em produção: _httpClient.PostAsJsonAsync)
        // Para fins didáticos, simulamos o comportamento
        await Task.Delay(10, cancellationToken); // simula latência de rede

        var response = SimulateGatewayResponse(request);

        // TRADUÇÃO: linguagem do gateway externo → linguagem do domínio
        return response.Status == "succeeded"
            ? PaymentResult.Success(response.ChargeId!)
            : PaymentResult.Failure(response.FailureMessage ?? "Falha desconhecida");
    }

    public async Task<PaymentResult> RefundAsync(
        string transactionId, Money amount,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        // Simulação de estorno bem-sucedido
        return PaymentResult.Success($"refund_{transactionId}");
    }

    // Simulação do gateway para fins didáticos
    private static ExternalChargeResponse SimulateGatewayResponse(ExternalChargeRequest req)
    {
        if (req.Token == "invalid_token")
            return new ExternalChargeResponse { Status = "failed", FailureMessage = "Token inválido" };

        return new ExternalChargeResponse
        {
            Status = "succeeded",
            ChargeId = $"ch_{Guid.NewGuid():N}"
        };
    }
}
