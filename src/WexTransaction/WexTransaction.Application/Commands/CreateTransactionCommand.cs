namespace WexTransaction.Application.Commands;

public record CreateTransactionCommand(
    string Description,
    DateTime Date,
    decimal Amount
) : IRequest<Guid>;
