namespace WexTransaction.Domain.Interfaces;

/// <summary>
/// Port: Serviço de consulta de transações com conversão de moeda.
/// Implementação: TransactionQueryService (Application layer).
/// Responsabilidade: Recuperar transação e aplicar lógica de conversão.
/// </summary>
public interface ITransactionQueryService
{
    Task<TransactionQueryResult?> GetTransactionWithConversionAsync(
        Guid transactionId,
        string country,
        string currency,
        CancellationToken cancellationToken);
}

public record TransactionQueryResult(
    Guid TransactionId,
    string Description,
    DateTime Date,
    decimal Amount,
    decimal TaxRate,
    decimal ConvertedValue);
