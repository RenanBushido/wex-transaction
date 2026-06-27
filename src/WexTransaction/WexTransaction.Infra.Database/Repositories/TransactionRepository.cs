namespace WexTransaction.Infra.Database.Repositories;

public class TransactionRepository(WexTransactionDbContext context) : BaseRepository<PurchaseTransaction>(context), ITransactionRepository
{
    #region Variables
    private readonly WexTransactionDbContext _context = context;

    #endregion

    #region Public Methods

    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
    {
        return await _context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(PurchaseTransaction transaction)
    {
        await _context.PurchaseTransactions.AddAsync(transaction);
    }

    public async Task SavePurchaseTransaction(PurchaseTransaction transaction)
    {
        await _context.PurchaseTransactions.AddAsync(transaction);
    }

    #endregion
}
