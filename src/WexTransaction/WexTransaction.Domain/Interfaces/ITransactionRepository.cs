namespace WexTransaction.Domain.Interfaces;

public interface ITransactionRepository
{
    Task SavePurchaseTransaction(PurchaseTransaction transaction);
}