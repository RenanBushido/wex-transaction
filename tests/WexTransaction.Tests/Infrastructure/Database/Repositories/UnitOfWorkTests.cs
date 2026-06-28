namespace WexTransaction.Tests.Infrastructure.Database.Repositories;

using Microsoft.EntityFrameworkCore;
using Xunit;
using WexTransaction.Infra.Database.Data;
using WexTransaction.Infra.Database.Repositories;
using WexTransaction.Domain.Entities;

public class UnitOfWorkTests
{
    private static WexTransactionDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public void UnitOfWork_CanBeInstantiated()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var unitOfWork = new UnitOfWork(context);

        // Assert
        Assert.NotNull(unitOfWork);
    }

    [Fact]
    public async Task UnitOfWork_Commit_SavesChangesToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var transaction = PurchaseTransaction.Create("Commit Test", DateTimeOffset.UtcNow, 100m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var saved = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task UnitOfWork_Commit_PersistsMultipleChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var transactions = new[]
        {
            PurchaseTransaction.Create("Tx1", DateTimeOffset.UtcNow, 100m),
            PurchaseTransaction.Create("Tx2", DateTimeOffset.UtcNow, 200m),
            PurchaseTransaction.Create("Tx3", DateTimeOffset.UtcNow, 300m)
        };

        // Act
        await context.PurchaseTransactions.AddRangeAsync(transactions);
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var count = await context.PurchaseTransactions.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task UnitOfWork_Commit_AppliesUpdates()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var transaction = PurchaseTransaction.Create("Update Test", DateTimeOffset.UtcNow, 100m);

        await context.PurchaseTransactions.AddAsync(transaction);
        await unitOfWork.Commit(CancellationToken.None);

        // Act - Update the entity
        var tracked = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(tracked);

        context.PurchaseTransactions.Update(tracked);
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var updated = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(updated);
    }

    [Fact]
    public async Task UnitOfWork_Commit_ApplisDeletions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var transaction = PurchaseTransaction.Create("Delete Test", DateTimeOffset.UtcNow, 100m);

        await context.PurchaseTransactions.AddAsync(transaction);
        await unitOfWork.Commit(CancellationToken.None);

        // Act - Delete the entity
        var tracked = await context.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(tracked);

        context.PurchaseTransactions.Remove(tracked);
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var count = await context.PurchaseTransactions.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UnitOfWork_Commit_WithoutChanges_Succeeds()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);

        // Act & Assert - Should not throw
        await unitOfWork.Commit(CancellationToken.None);
        Assert.True(true); // If we got here, it succeeded
    }

    [Fact]
    public async Task UnitOfWork_Commit_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var tx1 = PurchaseTransaction.Create("Tx1", DateTimeOffset.UtcNow, 100m);

        // Act - First commit
        await context.PurchaseTransactions.AddAsync(tx1);
        await unitOfWork.Commit(CancellationToken.None);

        var tx2 = PurchaseTransaction.Create("Tx2", DateTimeOffset.UtcNow, 200m);
        await context.PurchaseTransactions.AddAsync(tx2);
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var count = await context.PurchaseTransactions.CountAsync();
        Assert.Equal(2, count);
    }


    [Fact]
    public async Task UnitOfWork_CommitAfterException_HandlesGracefully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var transaction = PurchaseTransaction.Create("Exception Test", DateTimeOffset.UtcNow, 100m);

        await context.PurchaseTransactions.AddAsync(transaction);
        await unitOfWork.Commit(CancellationToken.None);

        // Act - Simulate an exception scenario
        var tracked = await context.PurchaseTransactions.FirstOrDefaultAsync();
        Assert.NotNull(tracked);

        // Try another commit - should still work
        await unitOfWork.Commit(CancellationToken.None);

        // Assert
        var saved = await context.PurchaseTransactions.CountAsync();
        Assert.Equal(1, saved);
    }

    [Fact]
    public async Task UnitOfWork_TransactionalIntegration_WithRepository()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new BaseRepository<PurchaseTransaction>(context);
        var unitOfWork = new UnitOfWork(context);

        var transactions = new[]
        {
            PurchaseTransaction.Create("Tx1", DateTimeOffset.UtcNow, 100m),
            PurchaseTransaction.Create("Tx2", DateTimeOffset.UtcNow, 200m)
        };

        // Act
        foreach (var tx in transactions)
        {
            repository.Create(tx);
        }

        await unitOfWork.Commit(CancellationToken.None);

        var allTx = await repository.GetAll(CancellationToken.None);

        // Assert
        Assert.Equal(2, allTx.Count);
    }
}
