namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionValidatorTests
{
    private readonly SaveTransactionValidator _validator;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee purchase";
    private const decimal ValidAmount = 200.24m;

    public SaveTransactionValidatorTests()
    {
        _validator = new SaveTransactionValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(string.Empty, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithDescriptionOver50Chars_ReturnsFalse()
    {
        // Arrange
        var longDescription = new string('x', 51);
        var command = new SaveTransactionCommand(longDescription, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithNullDate_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, default(DateTime), ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date");
    }

    [Fact]
    public void Validate_WithNullAmount_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, 0m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var command = new SaveTransactionCommand(string.Empty, default(DateTime), 0m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
