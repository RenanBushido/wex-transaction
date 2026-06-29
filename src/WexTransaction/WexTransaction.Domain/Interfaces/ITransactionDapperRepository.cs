namespace WexTransaction.Domain.Interfaces;

public interface ITransactionDapperRepository
{
    Task<PurchaseTransaction?> GetByIdAsync(Guid id);
}