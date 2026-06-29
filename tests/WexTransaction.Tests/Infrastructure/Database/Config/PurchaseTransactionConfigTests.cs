namespace WexTransaction.Tests.Infrastructure.Database.Config;

public class PurchaseTransactionConfigTests
{
    private static WexTransactionDbContext CreateConfiguredContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresEntityMapping()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var config = new PurchaseTransactionConfig();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));

        // Assert
        Assert.NotNull(entityType);
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresTableName()
    {
        // Arrange
        using var context = CreateConfiguredContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("tb_purchase_transaction", entityType.GetTableName());
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresIdProperty()
    {
        // Arrange
        using var context = CreateConfiguredContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));
        var idProperty = entityType?.FindProperty(nameof(PurchaseTransaction.Id));

        // Assert
        Assert.NotNull(idProperty);
        Assert.Equal("transaction_id", idProperty.GetColumnName());
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresDescriptionProperty()
    {
        // Arrange
        using var context = CreateConfiguredContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));
        var descProperty = entityType?.FindProperty(nameof(PurchaseTransaction.Description));

        // Assert
        Assert.NotNull(descProperty);
        Assert.Equal("transaction_description", descProperty.GetColumnName());
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresAmountProperty()
    {
        // Arrange
        using var context = CreateConfiguredContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));
        var amountProperty = entityType?.FindProperty(nameof(PurchaseTransaction.Amount));

        // Assert
        Assert.NotNull(amountProperty);
        Assert.Equal("transaction_amount", amountProperty.GetColumnName());
    }

    [Fact]
    public void PurchaseTransactionConfig_ConfiguresDateProperty()
    {
        // Arrange
        using var context = CreateConfiguredContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(PurchaseTransaction));
        var dateProperty = entityType?.FindProperty(nameof(PurchaseTransaction.TransactionDate));

        // Assert
        Assert.NotNull(dateProperty);
        Assert.Equal("transaction_date", dateProperty.GetColumnName());
    }

    [Fact]
    public async Task PurchaseTransactionConfig_PreservesValueObjectConversions()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var description = "Config Conversion Test";
        var amount = 999.99m;
        var date = new DateTime(2026, 6, 23, 12, 0, 0, DateTimeKind.Utc);

        var transaction = PurchaseTransaction.Create(description, date, amount);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queried);
        Assert.IsType<TransactionDescription>(queried.Description);
        Assert.IsType<decimal>(queried.Amount);
        Assert.Equal(description, queried.Description.Value);
        Assert.Equal(amount, queried.Amount);
    }

    [Fact]
    public async Task PurchaseTransactionConfig_HandlesSingleCharDescription()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var transaction = PurchaseTransaction.Create("X", DateTime.UtcNow, 1m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queried);
        Assert.Equal("X", queried.Description.Value);
    }

    [Fact]
    public async Task PurchaseTransactionConfig_HandlesMaxLengthDescription()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var maxDescription = new string('A', 50);
        var transaction = PurchaseTransaction.Create(maxDescription, DateTime.UtcNow, 1m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var queried = await context.PurchaseTransactions.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(queried);
        Assert.Equal(maxDescription, queried.Description.Value);
        Assert.Equal(50, queried.Description.Value.Length);
    }

    [Fact]
    public async Task PurchaseTransactionConfig_HandlesDecimalPrecision()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var amounts = new[] { 0.01m, 1.00m, 100.50m, 9999.99m };

        foreach (var amount in amounts)
        {
            // Act
            var transaction = PurchaseTransaction.Create($"Amount {amount}", DateTime.UtcNow, amount);
            await context.PurchaseTransactions.AddAsync(transaction);
        }

        await context.SaveChangesAsync();

        // Assert
        var queried = await context.PurchaseTransactions.ToListAsync();
        Assert.Equal(4, queried.Count);
        foreach (var item in queried)
        {
            Assert.Contains(item.Amount, amounts);
        }
    }

    [Fact]
    public async Task PurchaseTransactionConfig_HasKeyConfiguration()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var transaction1 = PurchaseTransaction.Create("Tx1", DateTime.UtcNow, 100m);
        var transaction2 = PurchaseTransaction.Create("Tx2", DateTime.UtcNow, 100m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction1);
        await context.PurchaseTransactions.AddAsync(transaction2);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotEqual(transaction1.Id, transaction2.Id);
        Assert.Equal(2, context.PurchaseTransactions.Count());
    }

    [Fact]
    public async Task PurchaseTransactionConfig_EnforcesRequiredProperties()
    {
        // Arrange
        using var context = CreateConfiguredContext();
        var transaction = PurchaseTransaction.Create("Required Test", DateTime.UtcNow, 100m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        var entry = context.Entry(transaction);

        // Assert
        Assert.NotEqual(Guid.Empty, (Guid)entry.Property(t => t.Id).CurrentValue!);
        Assert.NotNull((object?)entry.Property(t => t.Description).CurrentValue);
        Assert.NotNull((object?)entry.Property(t => t.Amount).CurrentValue);
        Assert.NotEqual(DateTime.MinValue, (DateTime)entry.Property(t => t.TransactionDate).CurrentValue!);
    }
}
