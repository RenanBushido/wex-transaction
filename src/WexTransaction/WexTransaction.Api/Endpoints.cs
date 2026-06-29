namespace WexTransaction.Api;

using MediatR;
using WexTransaction.Application.UseCases.GetPurchaseTransaction;
using WexTransaction.Application.UseCases.SavePurchaseTransaction;

public static class Endpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {



        var group = app.MapGroup("/api/v1")
            .WithTags("Transactions");

        group.MapPost("/transaction", SaveTransaction)
            .WithName("SaveTransaction")
            .WithDescription("Save a new purchase transaction")
            .Produces<SaveTransactionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/transaction/{id}/location/{country}-{currency}", GetTransaction)
            .WithName("GetTransaction")
            .WithDescription("Retrieve transaction with currency conversion")
            .Produces<GetPurchaseTransactionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> SaveTransaction(
        SaveTransactionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new SaveTransactionCommand(request.Description, request.Date, request.Amount);
        var transactionId = await mediator.Send(command, cancellationToken);

        return Results.Created($"/api/v1/transaction/{transactionId}", new SaveTransactionResponse(transactionId));
    }

    public static async Task<IResult> GetTransaction(
        Guid id,
        string country,
        string currency,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseTransactionRequest(id, country, currency);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return Results.NotFound($"Transaction {id} not found");

        return Results.Ok(result);
    }
}

public record SaveTransactionRequest(string Description, DateTime Date, decimal Amount);
public record SaveTransactionResponse(Guid TransactionId);
