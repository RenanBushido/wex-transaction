namespace WexTransaction.Tests.Domain.Exceptions;

using Xunit;
using WexTransaction.Domain.Exceptions;

public class InvalidAmountExceptionTests
{
    [Fact]
    public void InvalidAmountException_InheritsFromDomainException()
    {
        // Arrange
        var message = "Amount must be greater than zero";

        // Act
        var exception = new InvalidAmountException(message);

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void InvalidAmountException_StoresMessage()
    {
        // Arrange
        var message = "Amount cannot be negative";

        // Act
        var exception = new InvalidAmountException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidAmountException_CanBeThrown()
    {
        // Arrange
        var message = "Invalid amount: amount must be positive";

        // Act & Assert
        var exception = Assert.Throws<InvalidAmountException>((Action)(() =>
            throw new InvalidAmountException(message)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidAmountException_IsSealed()
    {
        // Arrange
        var exception = new InvalidAmountException("Test");

        // Act & Assert
        var type = exception.GetType();
        Assert.True(type.IsSealed, "InvalidAmountException should be sealed");
    }

    [Fact]
    public void InvalidAmountException_WithZeroAmountMessage()
    {
        // Arrange
        var message = "Amount must be greater than zero";

        // Act
        var exception = new InvalidAmountException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidAmountException_WithNegativeAmountMessage()
    {
        // Arrange
        var message = "Amount cannot be negative";

        // Act
        var exception = new InvalidAmountException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidAmountException_WithMaxValueMessage()
    {
        // Arrange
        var message = "Amount exceeds maximum allowed value";

        // Act
        var exception = new InvalidAmountException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidAmountException_PreservesMessageOnThrow()
    {
        // Arrange
        var message = "Transaction amount validation failed";

        // Act & Assert
        try
        {
            throw new InvalidAmountException(message);
        }
        catch (InvalidAmountException ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    [Fact]
    public void InvalidAmountException_ToString_IncludesExceptionType()
    {
        // Arrange
        var message = "Invalid amount";
        var exception = new InvalidAmountException(message);

        // Act
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("InvalidAmountException", exceptionString);
        Assert.Contains(message, exceptionString);
    }
}
