namespace WexTransaction.Infra.Database.Repositories;

public class BaseRepository<T>(WexTransactionDbContext context) : IBaseRepository<T> where T : BaseEntity
{
    #region Variables
    private readonly WexTransactionDbContext _context = context;

    #endregion

    #region Public Methods

    public void Create(T entity)
    {
        _context.Add(entity);
    }

    public void Update(T entity)
    {
        _context.Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Remove(entity);
    }

    public async Task<T?> Get(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<T>> GetAll(CancellationToken cancellationToken)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    #endregion
}