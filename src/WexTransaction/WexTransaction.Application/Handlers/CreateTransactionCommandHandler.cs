using WexTransaction.Domain.Interfaces;

namespace WexTransaction.Application.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTransactionCommandHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = PurchaseTransaction.Create(request.Description, request.Date, request.Amount);
        await _repository.AddAsync(transaction);
        await _unitOfWork.Commit(cancellationToken);

        // TODO: Phase 2B - publish TransactionCreatedEvent via injected IEventPublisher (deferred)

        return transaction.Id;
    }
}
