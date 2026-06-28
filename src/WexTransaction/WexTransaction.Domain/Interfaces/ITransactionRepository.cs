namespace WexTransaction.Domain.Interfaces;

public interface ITransactionRepository : IBaseRepository<PurchaseTransaction>
{
    Task SavePurchaseTransaction(PurchaseTransaction transaction);
    Task<PurchaseTransaction?> GetByIdAsync(Guid id);
    Task AddAsync(PurchaseTransaction transaction);
}