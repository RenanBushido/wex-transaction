namespace WexTransaction.Infra.Database.Data;

public class WexTransactionDbContext(DbContextOptions<WexTransactionDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }

    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WexTransactionDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
