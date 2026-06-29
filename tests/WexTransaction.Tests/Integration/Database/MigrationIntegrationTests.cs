namespace WexTransaction.Tests.Integration.Database;

public class MigrationIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15.18-alpine3.23")
            .WithDatabase("wextransaction_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();

        await _container.StartAsync();

        // Build the connection string
        _connectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    private WexTransactionDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
            .UseNpgsql(_connectionString)
            .ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new WexTransactionDbContext(options);
    }

    [Fact]
    public async Task ApplicationStartup_CreatesDatabaseAndAppliesMigrations()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act - Apply migrations
        await context.MigrateAsync();

        // Assert - Verify the database exists and is accessible
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect, "Database connection should succeed after migration");

        // Assert - Verify the purchase_transactions table exists
        var tableExists = await TableExistsAsync("tb_purchase_transaction");
        Assert.True(tableExists, "tb_purchase_transaction table should exist");

        // Assert - Verify table has the correct columns
        var columns = await GetTableColumnsAsync("tb_purchase_transaction");
        Assert.Contains("transaction_id", columns);
        Assert.Contains("transaction_description", columns);
        Assert.Contains("transaction_date", columns);
        Assert.Contains("transaction_amount", columns);
        Assert.Contains("CreatedAt", columns);
    }

    [Fact]
    public async Task ApplicationStartup_WithExistingDatabase_AppliesNewMigrations()
    {
        // Arrange - Apply initial migrations
        using var contextFirst = CreateDbContext();
        await contextFirst.MigrateAsync();

        // Verify initial state
        var tableExists = await TableExistsAsync("tb_purchase_transaction");
        Assert.True(tableExists, "tb_purchase_transaction table should exist after first migration");

        // Act - Run migrations again (should be idempotent)
        using var contextSecond = CreateDbContext();
        await contextSecond.MigrateAsync();

        // Assert - Verify no errors occurred and database is still accessible
        var canConnect = await contextSecond.Database.CanConnectAsync();
        Assert.True(canConnect, "Database connection should succeed after idempotent migration");

        // Assert - Verify table structure is preserved
        var columns = await GetTableColumnsAsync("tb_purchase_transaction");
        Assert.Contains("transaction_id", columns);
        Assert.Contains("transaction_description", columns);
        Assert.Contains("transaction_date", columns);
        Assert.Contains("transaction_amount", columns);

        // Assert - Verify table is still empty (no data corruption)
        using var contextVerify = CreateDbContext();
        var transactionCount = await contextVerify.PurchaseTransactions.CountAsync();
        Assert.Equal(0, transactionCount);
    }

    [Fact]
    public async Task ApplicationStartup_DatabaseMigrationAllowsInsertingData()
    {
        // Arrange
        using var context = CreateDbContext();
        await context.MigrateAsync();

        var transaction = PurchaseTransaction.Create("Test Transaction", DateTime.UtcNow, 100.50m);

        // Act
        await context.PurchaseTransactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = CreateDbContext();
        var savedTransaction = await verifyContext.PurchaseTransactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        Assert.NotNull(savedTransaction);
        Assert.Equal(transaction.Id, savedTransaction.Id);
        Assert.Equal("Test Transaction", savedTransaction.Description.Value);
        Assert.Equal(100.50m, (decimal)savedTransaction.Amount);
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        using var context = CreateDbContext();
        var connection = context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = 'public'
                    AND table_name = @tableName
                )";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            return result != null && (bool)result;
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task<List<string>> GetTableColumnsAsync(string tableName)
    {
        var columns = new List<string>();
        using var context = CreateDbContext();
        var connection = context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT column_name FROM information_schema.columns
                WHERE table_schema = 'public'
                AND table_name = @tableName
                ORDER BY ordinal_position";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }

        return columns;
    }
}
