// =============================================================================
// DDD - Value Object no contexto de Payment
// Representa o resultado de uma operação de pagamento.
// Este VO é a "linguagem" do domínio para falar sobre pagamentos —
// não expõe detalhes do gateway externo (parte da ACL).
// SOLID - SRP: encapsula apenas o resultado de uma transação.
// OO - Abstração: o domínio não sabe se usou Stripe, PagSeguro, etc.
// =============================================================================

using Domain.Common;

namespace Domain.Payment.ValueObjects;

/// <summary>
/// Value Object que representa o resultado de uma cobrança.
/// Criado pelo PaymentServiceAdapter (ACL), traduzido para a linguagem do domínio.
/// </summary>
public sealed class PaymentResult : ValueObject
{
    public bool IsSuccess { get; }
    public string? TransactionId { get; }
    public string? ErrorMessage { get; }
    public DateTime ProcessedAt { get; }

    private PaymentResult(bool isSuccess, string? transactionId, string? errorMessage)
    {
        IsSuccess = isSuccess;
        TransactionId = transactionId;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }

    // Factory methods — expõem criação legível (Ubiquitous Language)
    public static PaymentResult Success(string transactionId) =>
        new(true, transactionId, null);

    public static PaymentResult Failure(string errorMessage) =>
        new(false, null, errorMessage);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsSuccess;
        yield return TransactionId ?? string.Empty;
        yield return ErrorMessage ?? string.Empty;
    }
}
