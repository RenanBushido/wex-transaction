namespace WexTransaction.Application.PurchaseTransaction.SaveTransaction;

public class SaveTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ITransactionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = Domain.Entities.PurchaseTransaction.Create(request.Description, request.Date, request.Amount);
        await _repository.SavePurchaseTransaction(transaction);
        await _unitOfWork.Commit(cancellationToken);

        // TODO: Phase 2B - publish TransactionCreatedEvent via injected IEventPublisher (deferred)

        return transaction.Id;
    }
}
