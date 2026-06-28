namespace WexTransaction.Tests.Domain.Exceptions;

public class InvalidTransactionDateExceptionTests
{
    [Fact]
    public void InvalidTransactionDateException_InheritsFromDomainException()
    {
        // Arrange
        var message = "Transaction date must be in UTC";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void InvalidTransactionDateException_StoresMessage()
    {
        // Arrange
        var message = "Transaction date cannot be in the future";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_CanBeThrown()
    {
        // Arrange
        var message = "Transaction date must not be a default value";

        // Act & Assert
        var exception = Assert.Throws<InvalidTransactionDateException>((Action)(() =>
            throw new InvalidTransactionDateException(message)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_IsSealed()
    {
        // Arrange
        var exception = new InvalidTransactionDateException("Test");

        // Act & Assert
        var type = exception.GetType();
        Assert.True(type.IsSealed, "InvalidTransactionDateException should be sealed");
    }

    [Fact]
    public void InvalidTransactionDateException_WithDefaultValueMessage()
    {
        // Arrange
        var message = "Transaction date must not be a default value";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_WithUTCOffsetMessage()
    {
        // Arrange
        var message = "Transaction date must be in UTC (offset must be zero)";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_WithFutureTimeMessage()
    {
        // Arrange
        var message = "Transaction date cannot be in the future";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_WithPastLimitMessage()
    {
        // Arrange
        var message = "Transaction date is too far in the past";

        // Act
        var exception = new InvalidTransactionDateException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidTransactionDateException_PreservesMessageOnThrow()
    {
        // Arrange
        var message = "Transaction date validation failed";

        // Act & Assert
        try
        {
            throw new InvalidTransactionDateException(message);
        }
        catch (InvalidTransactionDateException ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    [Fact]
    public void InvalidTransactionDateException_ToString_IncludesExceptionType()
    {
        // Arrange
        var message = "Invalid transaction date";
        var exception = new InvalidTransactionDateException(message);

        // Act
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("InvalidTransactionDateException", exceptionString);
        Assert.Contains(message, exceptionString);
    }
}
