// =============================================================================
// INFRAESTRUTURA - Anti-Corruption Layer
// DTOs que representam a linguagem do gateway externo (ex: Stripe, PagSeguro).
// Ficam AQUI, nunca no domínio. O domínio não conhece esses tipos.
// =============================================================================

namespace Infrastructure.AntiCorruptionLayer;

/// <summary>
/// DTO de request para o gateway externo de pagamento.
/// Representa a estrutura esperada pela API externa.
/// </summary>
internal sealed class ExternalChargeRequest
{
    public string OrderReference { get; set; } = string.Empty;
    public long AmountCents { get; set; }       // gateway usa centavos inteiros
    public string Currency { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// DTO de response do gateway externo de pagamento.
/// </summary>
internal sealed class ExternalChargeResponse
{
    public string? ChargeId { get; set; }
    public string Status { get; set; } = string.Empty;   // "succeeded" | "failed"
    public string? FailureMessage { get; set; }
}
