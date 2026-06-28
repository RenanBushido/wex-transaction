namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

using Xunit;
using WexTransaction.Application.UseCases.SavePurchaseTransaction;

/// <summary>
/// Test: SaveTransactionCommand - Validates command structure and properties
/// </summary>
public class SaveTransactionCommandTests
{
    [Fact]
    public void SaveTransactionCommand_ImplementsIRequest()
    {
        // Arrange
        var command = new SaveTransactionCommand(
            Description: "Test Transaction",
            Date: DateTime.UtcNow,
            Amount: 100.50m
        );

        // Act & Assert
        Assert.NotNull(command);
        Assert.IsAssignableFrom<IRequest<Guid>>(command);
    }

    [Fact]
    public void SaveTransactionCommand_HasCorrectProperties()
    {
        // Arrange
        var description = "Test Description";
        var date = DateTime.UtcNow;
        var amount = 250.75m;

        // Act
        var command = new SaveTransactionCommand(
            Description: description,
            Date: date,
            Amount: amount
        );

        // Assert
        Assert.Equal(description, command.Description);
        Assert.Equal(date, command.Date);
        Assert.Equal(amount, command.Amount);
    }

    [Fact]
    public void SaveTransactionCommand_IsRecord_Immutable()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var command1 = new SaveTransactionCommand("Test", date, 100m);
        var command2 = command1 with { }; // Use 'with' to create exact copy

        // Act & Assert
        Assert.Equal(command1, command2); // Record equality
    }

    [Fact]
    public void SaveTransactionCommand_CanBeCreatedWithMinimalData()
    {
        // Arrange & Act
        var command = new SaveTransactionCommand(
            Description: "Min Test",
            Date: DateTime.MinValue,
            Amount: 0.01m
        );

        // Assert
        Assert.NotNull(command);
        Assert.Equal("Min Test", command.Description);
    }

    [Fact]
    public void SaveTransactionCommand_CanBeCreatedWithLargeAmount()
    {
        // Arrange & Act
        var command = new SaveTransactionCommand(
            Description: "Large Amount",
            Date: DateTime.UtcNow,
            Amount: 999999999.99m
        );

        // Assert
        Assert.NotNull(command);
        Assert.Equal(999999999.99m, command.Amount);
    }

    [Fact]
    public void SaveTransactionCommand_WithSpecialCharacters_Accepted()
    {
        // Arrange
        var description = "Test™ Description © 2026 - Special: !@#$%^&*()";

        // Act
        var command = new SaveTransactionCommand(
            Description: description,
            Date: DateTime.UtcNow,
            Amount: 100m
        );

        // Assert
        Assert.Equal(description, command.Description);
    }

    [Fact]
    public void SaveTransactionCommand_WithDifferentDates_CreatesCorrectCommand()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act
        var commandPast = new SaveTransactionCommand("Past", pastDate, 100m);
        var commandFuture = new SaveTransactionCommand("Future", futureDate, 100m);

        // Assert
        Assert.Equal(pastDate, commandPast.Date);
        Assert.Equal(futureDate, commandFuture.Date);
        Assert.NotEqual(commandPast.Date, commandFuture.Date);
    }
}
