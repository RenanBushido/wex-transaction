namespace WexTransaction.Tests.Application.UseCases.GetPurchaseTransaction;

/// <summary>
/// Test: GetPurchaseTransactionRequest - Validates query structure and properties
/// </summary>
public class GetPurchaseTransactionRequestTests
{
    [Fact]
    public void GetPurchaseTransactionRequest_ImplementsIRequest()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetPurchaseTransactionRequest(
            TransactionId: transactionId,
            Country: "Brazil",
            Currency: "BRL"
        );

        // Act & Assert
        Assert.NotNull(request);
        Assert.IsAssignableFrom<IRequest<GetPurchaseTransactionResponse>>(request);
    }

    [Fact]
    public void GetPurchaseTransactionRequest_HasCorrectProperties()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var country = "United States";
        var currency = "USD";

        // Act
        var request = new GetPurchaseTransactionRequest(
            TransactionId: transactionId,
            Country: country,
            Currency: currency
        );

        // Assert
        Assert.Equal(transactionId, request.TransactionId);
        Assert.Equal(country, request.Country);
        Assert.Equal(currency, request.Currency);
    }

    [Fact]
    public void GetPurchaseTransactionRequest_IsRecord_Immutable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request1 = new GetPurchaseTransactionRequest(id, "Brazil", "BRL");
        var request2 = new GetPurchaseTransactionRequest(id, "Brazil", "BRL");

        // Act & Assert
        Assert.Equal(request1, request2); // Record equality
    }

    [Fact]
    public void GetPurchaseTransactionRequest_WithValidGuid_Accepted()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act
        var request = new GetPurchaseTransactionRequest(
            TransactionId: validGuid,
            Country: "Brazil",
            Currency: "BRL"
        );

        // Assert
        Assert.Equal(validGuid, request.TransactionId);
        Assert.NotEqual(Guid.Empty, request.TransactionId);
    }

    [Fact]
    public void GetPurchaseTransactionRequest_WithDifferentCurrencies_CreatesCorrectRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var currencies = new[] { "USD", "BRL", "EUR", "JPY", "GBP" };

        // Act & Assert
        foreach (var currency in currencies)
        {
            var request = new GetPurchaseTransactionRequest(id, "Test Country", currency);
            Assert.Equal(currency, request.Currency);
        }
    }

    [Fact]
    public void GetPurchaseTransactionRequest_WithDifferentCountries_CreatesCorrectRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var countries = new[] { "Brazil", "Japan", "United Kingdom", "Germany", "Canada" };

        // Act & Assert
        foreach (var country in countries)
        {
            var request = new GetPurchaseTransactionRequest(id, country, "USD");
            Assert.Equal(country, request.Country);
        }
    }

    [Fact]
    public void GetPurchaseTransactionRequest_WithSpecialCharacters_Accepted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var country = "São Paulo - Brazil";
        var currency = "BRL";

        // Act
        var request = new GetPurchaseTransactionRequest(id, country, currency);

        // Assert
        Assert.Equal(country, request.Country);
    }

    [Fact]
    public void GetPurchaseTransactionRequest_CanBeCreatedMultipleTimes_WithUniqueIds()
    {
        // Arrange & Act
        var request1 = new GetPurchaseTransactionRequest(Guid.NewGuid(), "Brazil", "BRL");
        var request2 = new GetPurchaseTransactionRequest(Guid.NewGuid(), "Brazil", "BRL");

        // Assert
        Assert.NotEqual(request1.TransactionId, request2.TransactionId);
    }

    [Fact]
    public void GetPurchaseTransactionResponse_HasCorrectProperties()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var description = "Test Transaction";
        var date = DateTime.UtcNow;
        var amount = 100.50m;
        var taxRate = 5.25m;
        var convertedValue = 526.25m;

        // Act
        var response = new GetPurchaseTransactionResponse(
            TransactionId: transactionId,
            Description: description,
            Date: date,
            Amount: amount,
            TaxRate: taxRate,
            ConvertedValue: convertedValue
        );

        // Assert
        Assert.Equal(transactionId, response.TransactionId);
        Assert.Equal(description, response.Description);
        Assert.Equal(date, response.Date);
        Assert.Equal(amount, response.Amount);
        Assert.Equal(taxRate, response.TaxRate);
        Assert.Equal(convertedValue, response.ConvertedValue);
    }

    [Fact]
    public void GetPurchaseTransactionResponse_IsRecord_Immutable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var response1 = new GetPurchaseTransactionResponse(id, "Test", date, 100m, 5m, 500m);
        var response2 = response1 with { }; // Use 'with' to create exact copy

        // Act & Assert
        Assert.Equal(response1, response2);
    }
}
