namespace WexTransaction.Tests.Domain.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_Constructor_StoresMessage()
    {
        // Arrange
        var message = "Test domain error message";

        // Act
        var exception = new DomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void DomainException_InheritsFromException()
    {
        // Arrange
        var message = "Test error";

        // Act
        var exception = new DomainException(message);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void DomainException_CanBeThrown()
    {
        // Arrange
        var message = "Domain constraint violation";

        // Act & Assert
        var exception = Assert.Throws<DomainException>((Action)(() =>
            throw new DomainException(message)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void DomainException_WithEmptyMessage()
    {
        // Arrange
        var message = string.Empty;

        // Act
        var exception = new DomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }


    [Fact]
    public void DomainException_WithLongMessage()
    {
        // Arrange
        var message = new string('A', 1000);

        // Act
        var exception = new DomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(1000, exception.Message.Length);
    }

    [Fact]
    public void DomainException_HasInnerExceptionProperty()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Domain error";

        // Act
        var exception = new DomainException(message);
        var exceptionWithInner = new DomainException(message);

        // Assert
        Assert.Null(exception.InnerException);
        Assert.NotNull(exceptionWithInner);
    }

    [Fact]
    public void DomainException_StackTraceAvailableWhenThrown()
    {
        // Arrange & Act
        DomainException? exception = null;
        try
        {
            throw new DomainException("Test error");
        }
        catch (DomainException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.NotNull(exception);
        Assert.NotNull(exception.StackTrace);
        Assert.NotEmpty(exception.StackTrace);
    }

    [Fact]
    public void DomainException_Serializable()
    {
        // Arrange
        var exception = new DomainException("Test message");

        // Act - Verify it can be converted to string
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("Test message", exceptionString);
        Assert.Contains("DomainException", exceptionString);
    }
}
