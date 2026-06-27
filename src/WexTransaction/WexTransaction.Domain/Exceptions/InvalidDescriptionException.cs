namespace WexTransaction.Domain.Exceptions;

public sealed class InvalidDescriptionException(string message) : DomainException(message)
{
}
