namespace WexTransaction.Domain.Exceptions;

public sealed class CurrencyConversionUnavailableException(string message) : DomainException(message)
{
}
