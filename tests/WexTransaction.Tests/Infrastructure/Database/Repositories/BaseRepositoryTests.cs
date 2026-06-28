namespace WexTransaction.Tests.Infrastructure.Database.Repositories;

public class BaseRepositoryTests
{
    private static WexTransactionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public void BaseRepository_CanBeInstantiated()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var repository = new BaseRepository<PurchaseTransaction>(context);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void BaseRepository_Create_AddsEntityToContext()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var transaction = PurchaseTransaction.Create("Create Test", DateTimeOffset.UtcNow, 100m);

        // Act
        repository.Create(transaction);
        context.SaveChanges();

        // Assert
        Assert.Single(context.PurchaseTransactions);
        Assert.Equal(transaction.Id, context.PurchaseTransactions.First().Id);
    }

    [Fact]
    public void BaseRepository_Update_ModifiesEntityInContext()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var transaction = PurchaseTransaction.Create("Update Test", DateTimeOffset.UtcNow, 100m);

        repository.Create(transaction);
        context.SaveChanges();

        // Act
        var tracked = context.PurchaseTransactions.First();
        repository.Update(tracked);
        context.SaveChanges();

        // Assert
        Assert.Single(context.PurchaseTransactions);
    }

    [Fact]
    public void BaseRepository_Delete_RemovesEntityFromContext()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var transaction = PurchaseTransaction.Create("Delete Test", DateTimeOffset.UtcNow, 100m);

        repository.Create(transaction);
        context.SaveChanges();

        // Act
        var tracked = context.PurchaseTransactions.First();
        repository.Delete(tracked);
        context.SaveChanges();

        // Assert
        Assert.Empty(context.PurchaseTransactions);
    }

    [Fact]
    public async Task BaseRepository_Get_ReturnsEntityById()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var transaction = PurchaseTransaction.Create("Get Test", DateTimeOffset.UtcNow, 100m);

        repository.Create(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.Get(transaction.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
    }

    [Fact]
    public async Task BaseRepository_Get_ReturnsNullWhenNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.Get(nonExistentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BaseRepository_GetAll_ReturnsAllEntities()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var transactions = new[]
        {
            PurchaseTransaction.Create("Tx1", DateTimeOffset.UtcNow, 100m),
            PurchaseTransaction.Create("Tx2", DateTimeOffset.UtcNow, 200m),
            PurchaseTransaction.Create("Tx3", DateTimeOffset.UtcNow, 300m)
        };

        foreach (var tx in transactions)
        {
            repository.Create(tx);
        }

        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAll(CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task BaseRepository_GetAll_ReturnsEmptyListWhenNoEntities()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);

        // Act
        var result = await repository.GetAll(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }


    [Fact]
    public async Task BaseRepository_MultipleOperations_WorkInSequence()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var tx1 = PurchaseTransaction.Create("Tx1", DateTimeOffset.UtcNow, 100m);

        // Act - Create
        repository.Create(tx1);
        await context.SaveChangesAsync();
        Assert.Single(context.PurchaseTransactions);

        // Act - Get
        var retrieved = await repository.Get(tx1.Id, CancellationToken.None);
        Assert.NotNull(retrieved);

        // Act - Update
        repository.Update(retrieved);
        await context.SaveChangesAsync();
        Assert.Single(context.PurchaseTransactions);

        // Act - Delete
        repository.Delete(retrieved);
        await context.SaveChangesAsync();
        Assert.Empty(context.PurchaseTransactions);
    }
}
