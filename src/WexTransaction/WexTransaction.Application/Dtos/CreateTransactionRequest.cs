namespace WexTransaction.Application.Dtos;

public record CreateTransactionRequest(
    string Description,
    DateTime Date,
    decimal Amount
);
