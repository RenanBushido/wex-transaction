namespace WexTransaction.Domain.Interfaces;
public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken);
}