namespace WexTransaction.Tests.Infrastructure.Database.Repositories;

public class TransactionRepositoryTests
{
    private static WexTransactionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionExists_ReturnsTransaction()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transaction = PurchaseTransaction.Create("Test", DateTimeOffset.UtcNow, 100m);
        context.PurchaseTransactions.Add(transaction);
        await unitOfWork.Commit(default);

        var result = await repository.GetByIdAsync(transaction.Id);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task SavePurchaseTransaction_AddsTransactionToContext()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transaction = PurchaseTransaction.Create("Test", DateTime.UtcNow, 100m);
        await repository.SavePurchaseTransaction(transaction);
        await unitOfWork.Commit(default);

        var entry = context.Entry(transaction);
        Assert.True(entry.State == EntityState.Added || entry.State == EntityState.Unchanged);
    }

    [Fact]
    public async Task SavePurchaseTransaction_PersistsTransactionToDatabase()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transaction = PurchaseTransaction.Create("Test", DateTime.UtcNow, 100m);
        await repository.SavePurchaseTransaction(transaction);
        await unitOfWork.Commit(default);

        var saved = await context.PurchaseTransactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        Assert.NotNull(saved);
        Assert.Equal(transaction.Id, saved.Id);
    }

    [Fact]
    public async Task GetByIdAsync_MultipleTransactions_ReturnsCorrectOne()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transaction1 = PurchaseTransaction.Create("Test 1", DateTime.UtcNow, 100m);
        var transaction2 = PurchaseTransaction.Create("Test 2", DateTime.UtcNow, 200m);

        await repository.SavePurchaseTransaction(transaction1);
        await repository.SavePurchaseTransaction(transaction2);
        await unitOfWork.Commit(default);

        var result = await repository.GetByIdAsync(transaction2.Id);

        Assert.NotNull(result);
        Assert.Equal(transaction2.Id, result.Id);
    }

    [Fact]
    public async Task SavePurchaseTransaction_MultipleTransactions_SavesAll()
    {
        using var context = CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transaction1 = PurchaseTransaction.Create("Test 1", DateTime.UtcNow, 100m);
        var transaction2 = PurchaseTransaction.Create("Test 2", DateTime.UtcNow, 200m);

        await repository.SavePurchaseTransaction(transaction1);
        await repository.SavePurchaseTransaction(transaction2);
        await unitOfWork.Commit(default);

        var count = await context.PurchaseTransactions.CountAsync();

        Assert.Equal(2, count);
    }
}
