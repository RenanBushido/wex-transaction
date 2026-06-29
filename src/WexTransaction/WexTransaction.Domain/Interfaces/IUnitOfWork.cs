namespace WexTransaction.Domain.Interfaces;
public interface IUnitOfWork
{
    ITransactionDapperRepository RepoTransactionDapper {get;}
    ITransactionRepository RepoTransaction {get;}
    Task Commit(CancellationToken cancellationToken);
}