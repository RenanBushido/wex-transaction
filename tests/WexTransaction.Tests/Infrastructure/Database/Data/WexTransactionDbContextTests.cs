namespace WexTransaction.Tests.Infrastructure.Database.Data;

public class WexTransactionDbContextTests
{
    private static WexTransactionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public void WexTransactionDbContext_CanBeInstantiated()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void WexTransactionDbContext_HasPurchaseTransactionsDbSet()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var dbSet = context.PurchaseTransactions;

        // Assert
        Assert.NotNull(dbSet);
        Assert.IsAssignableFrom<DbSet<PurchaseTransaction>>(dbSet);
    }

    [Fact]
    public async Task WexTransactionDbContext_CanAddTransaction()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = PurchaseTransaction.Create("Test", DateTime.UtcNow, 100m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Assert
        Assert.Single(context.PurchaseTransactions);
        Assert.Equal(transaction.Id, context.PurchaseTransactions.First().Id);
    }

    [Fact]
    public async Task WexTransactionDbContext_CanQueryTransaction()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = PurchaseTransaction.Create("Test Query", DateTime.UtcNow, 250m);
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Act
        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);

        // Assert
        Assert.NotNull(queried);
        Assert.Equal(transaction.Id, queried.Id);
    }

    [Fact]
    public async Task WexTransactionDbContext_CanUpdateTransaction()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = PurchaseTransaction.Create("Original", DateTime.UtcNow, 100m);
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Act
        var tracked = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(tracked);

        context.PurchaseTransactions.Update(tracked);
        await context.SaveChangesAsync();

        // Assert
        Assert.Single(context.PurchaseTransactions);
    }

    [Fact]
    public async Task WexTransactionDbContext_CanDeleteTransaction()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = PurchaseTransaction.Create("To Delete", DateTime.UtcNow, 100m);
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Act
        var tracked = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(tracked);

        context.PurchaseTransactions.Remove(tracked);
        await context.SaveChangesAsync();

        // Assert
        Assert.Empty(context.PurchaseTransactions);
    }

    [Fact]
    public async Task WexTransactionDbContext_AppliesConfigurationsFromAssembly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = PurchaseTransaction.Create("Config Test", DateTime.UtcNow, 100m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queried);
        Assert.Equal(transaction.Description.Value, queried.Description.Value);
        Assert.Equal(transaction.Amount, queried.Amount);
    }

    [Fact]
    public async Task WexTransactionDbContext_CanAddMultipleTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transactions = new[]
        {
            PurchaseTransaction.Create("Tx1", DateTime.UtcNow, 100m),
            PurchaseTransaction.Create("Tx2", DateTime.UtcNow, 200m),
            PurchaseTransaction.Create("Tx3", DateTime.UtcNow, 300m)
        };

        // Act
        await context.PurchaseTransactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(3, context.PurchaseTransactions.Count());
    }

    [Fact]
    public async Task WexTransactionDbContext_PreservesTransactionData()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var description = "Data Preservation Test";
        var date = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
        var amount = 123.45m;

        var transaction = PurchaseTransaction.Create(description, date, amount);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queried);
        Assert.Equal(description, queried.Description.Value);
        Assert.Equal(date, queried.TransactionDate);
        Assert.Equal(amount, queried.Amount);
    }
}
