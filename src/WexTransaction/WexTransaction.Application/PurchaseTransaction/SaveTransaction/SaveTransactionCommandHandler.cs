namespace WexTransaction.Application.PurchaseTransaction.SaveTransaction;

using WexTransaction.Application.Events.DomainEvents;

public class SaveTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEventPublisher eventPublisher) : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ITransactionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = Domain.Entities.PurchaseTransaction.Create(request.Description, request.Date, request.Amount);
        await _repository.SavePurchaseTransaction(transaction);
        await _unitOfWork.Commit(cancellationToken);

        var transactionCreatedEvent = new TransactionCreatedEvent(
            AggregateId: transaction.Id,
            OccurredAt: DateTimeOffset.UtcNow,
            Description: (string)transaction.Description,
            Amount: (decimal)transaction.Amount,
            Date: DateTime.UtcNow
        );
        await _eventPublisher.PublishAsync(transactionCreatedEvent, cancellationToken);

        return transaction.Id;
    }
}
