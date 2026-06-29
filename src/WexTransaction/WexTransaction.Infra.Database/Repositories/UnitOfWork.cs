namespace WexTransaction.Infra.Database.Repositories;

public class UnitOfWork(WexTransactionDbContext context) : IUnitOfWork, IDisposable
{
    #region Variables
    private readonly WexTransactionDbContext _context = context;

    #endregion

    #region Properties
    public ITransactionDapperRepository RepoTransactionDapper => throw new NotImplementedException();
    public ITransactionRepository RepoTransaction => throw new NotImplementedException();

    #endregion

    #region Public Methods

    public async Task Commit(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}