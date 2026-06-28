namespace WexTransaction.Tests.Domain.Exceptions;

using Xunit;
using WexTransaction.Domain.Exceptions;

public class CurrencyConversionUnavailableExceptionTests
{
    [Fact]
    public void CurrencyConversionUnavailableException_InheritsFromDomainException()
    {
        // Arrange
        var message = "Exchange rate data is not available";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_StoresMessage()
    {
        // Arrange
        var message = "Currency conversion service is temporarily unavailable";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_CanBeThrown()
    {
        // Arrange
        var message = "No exchange rate data available for the requested date and currency";

        // Act & Assert
        var exception = Assert.Throws<CurrencyConversionUnavailableException>((Action)(() =>
            throw new CurrencyConversionUnavailableException(message)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_IsSealed()
    {
        // Arrange
        var exception = new CurrencyConversionUnavailableException("Test");

        // Act & Assert
        var type = exception.GetType();
        Assert.True(type.IsSealed, "CurrencyConversionUnavailableException should be sealed");
    }

    [Fact]
    public void CurrencyConversionUnavailableException_WithNoDataMessage()
    {
        // Arrange
        var message = "No exchange rate data available for the requested parameters";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_WithServiceUnavailableMessage()
    {
        // Arrange
        var message = "Currency conversion service is currently unavailable";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_WithSpecificCurrencyMessage()
    {
        // Arrange
        var message = "No exchange rate data available for currency JPY on date 2026-06-28";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_WithTimeoutMessage()
    {
        // Arrange
        var message = "Currency conversion request timed out";

        // Act
        var exception = new CurrencyConversionUnavailableException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_PreservesMessageOnThrow()
    {
        // Arrange
        var message = "External API returned no data for currency conversion";

        // Act & Assert
        try
        {
            throw new CurrencyConversionUnavailableException(message);
        }
        catch (CurrencyConversionUnavailableException ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    [Fact]
    public void CurrencyConversionUnavailableException_ToString_IncludesExceptionType()
    {
        // Arrange
        var message = "Currency conversion unavailable";
        var exception = new CurrencyConversionUnavailableException(message);

        // Act
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("CurrencyConversionUnavailableException", exceptionString);
        Assert.Contains(message, exceptionString);
    }

    [Fact]
    public void CurrencyConversionUnavailableException_CanCommunicateRetryableScenario()
    {
        // Arrange
        var exception = new CurrencyConversionUnavailableException("Temporary service unavailability");

        // Act & Assert
        Assert.NotNull(exception);
        Assert.Contains("Temporary", exception.Message);
    }
}
