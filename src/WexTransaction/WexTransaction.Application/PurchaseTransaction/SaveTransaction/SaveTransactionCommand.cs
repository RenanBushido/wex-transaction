namespace WexTransaction.Application.Commands;

public sealed record CreateTransactionCommand(
    string Description,
    DateTime Date,
    decimal Amount
) : IRequest<Guid>;
