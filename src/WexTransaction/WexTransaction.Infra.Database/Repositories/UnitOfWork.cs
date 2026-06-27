namespace WexTransaction.Infra.Database.Repositories;

public class UnitOfWork(WexTransactionDbContext context) : IUnitOfWork
{
    #region Variables
    private readonly WexTransactionDbContext _context = context;
    
    #endregion

    #region Public Methods

    public async Task Commit(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}