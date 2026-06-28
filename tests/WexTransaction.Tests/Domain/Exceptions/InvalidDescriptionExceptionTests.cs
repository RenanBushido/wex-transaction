namespace WexTransaction.Tests.Domain.Exceptions;

public class InvalidDescriptionExceptionTests
{
    [Fact]
    public void InvalidDescriptionException_InheritsFromDomainException()
    {
        // Arrange
        var message = "Description cannot be empty";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void InvalidDescriptionException_StoresMessage()
    {
        // Arrange
        var message = "Description must not be null or whitespace";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_CanBeThrown()
    {
        // Arrange
        var message = "Transaction description is required";

        // Act & Assert
        var exception = Assert.Throws<InvalidDescriptionException>((Action)(() =>
            throw new InvalidDescriptionException(message)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_IsSealed()
    {
        // Arrange
        var exception = new InvalidDescriptionException("Test");

        // Act & Assert
        var type = exception.GetType();
        Assert.True(type.IsSealed, "InvalidDescriptionException should be sealed");
    }

    [Fact]
    public void InvalidDescriptionException_WithEmptyStringMessage()
    {
        // Arrange
        var message = "Description cannot be an empty string";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_WithWhitespaceMessage()
    {
        // Arrange
        var message = "Description cannot contain only whitespace";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_WithLengthExceededMessage()
    {
        // Arrange
        var message = "Description exceeds maximum length of 500 characters";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_WithNullMessage()
    {
        // Arrange
        var message = "Description must not be null";

        // Act
        var exception = new InvalidDescriptionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDescriptionException_PreservesMessageOnThrow()
    {
        // Arrange
        var message = "Transaction description validation failed";

        // Act & Assert
        try
        {
            throw new InvalidDescriptionException(message);
        }
        catch (InvalidDescriptionException ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    [Fact]
    public void InvalidDescriptionException_ToString_IncludesExceptionType()
    {
        // Arrange
        var message = "Invalid description";
        var exception = new InvalidDescriptionException(message);

        // Act
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("InvalidDescriptionException", exceptionString);
        Assert.Contains(message, exceptionString);
    }
}
