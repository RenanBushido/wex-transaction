namespace WexTransaction.Domain.Exceptions;

public sealed class InvalidTransactionDateException(string message) : DomainException(message)
{
}
