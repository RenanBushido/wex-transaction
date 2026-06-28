namespace WexTransaction.Application.Services;

public class CreateTransactionUseCase : ICreateTransactionUseCase
{
    private readonly IMediator _mediator;

    public CreateTransactionUseCase(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Guid> ExecuteAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(request.Description, request.Date, request.Amount);
        return await _mediator.Send(command, cancellationToken);
    }
}
