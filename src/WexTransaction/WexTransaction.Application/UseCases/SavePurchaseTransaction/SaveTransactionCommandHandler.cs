namespace WexTransaction.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionCommandHandler(
    ITransactionRepository repository,    
    IUnitOfWork unitOfWork) : IRequestHandler<SaveTransactionCommand, Guid>
{
    #region Variables
    private readonly ITransactionRepository _repository = repository;    
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    #endregion

    #region Public Methods
    public async Task<Guid> Handle(SaveTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = PurchaseTransaction.Create(request.Description, request.Date, request.Amount);
        await _repository.SavePurchaseTransaction(transaction);
        await _unitOfWork.Commit(cancellationToken);

        return transaction.Id;
    }

    #endregion
}
