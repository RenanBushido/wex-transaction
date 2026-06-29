namespace WexTransaction.Tests.Infrastructure.Services.RatesExchange.Clients;
public class ITreasuryExchangeRateClientTests
{
    [Fact]
    public void ITreasuryExchangeRateClient_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "ITreasuryExchangeRateClient should be an interface");
    }

    [Fact]
    public void ITreasuryExchangeRateClient_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "ITreasuryExchangeRateClient should be public");
    }

    [Fact]
    public void ITreasuryExchangeRateClient_FollowsNamingConvention()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var name = interfaceType.Name;

        // Assert
        Assert.True(name.StartsWith("I"), "Interface should start with 'I'");
    }

    [Fact]
    public void ITreasuryExchangeRateClient_DefinesGetExchangeRatesAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>),
            "GetExchangeRatesAsync should return Task<TreasuryRatesResponse>");
    }

    [Fact]
    public void ITreasuryExchangeRateClient_GetMethodHasGetAttribute()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var getAttributes = method?.GetCustomAttributes(typeof(GetAttribute), false);

        // Assert
        Assert.NotNull(getAttributes);
        Assert.NotEmpty(getAttributes);
    }

    [Fact]
    public void ITreasuryExchangeRateClient_GetAttributeHasCorrectPath()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var getAttributes = method?.GetCustomAttributes(typeof(GetAttribute), false);
        var getAttr = getAttributes?.FirstOrDefault() as GetAttribute;

        // Assert
        Assert.NotNull(getAttr);
        Assert.Equal("/v1/accounting/od/rates_of_exchange", getAttr.Path);
    }

    [Fact]
    public void ITreasuryExchangeRateClient_FilterParameterHasQueryAttribute()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var filterParam = method?.GetParameters().FirstOrDefault(p => p.Name == "filter");
        var queryAttributes = filterParam?.GetCustomAttributes(typeof(QueryAttribute), false);

        // Assert
        Assert.NotNull(queryAttributes);
        Assert.NotEmpty(queryAttributes);
    }

    [Fact]
    public void ITreasuryExchangeRateClient_SortParameterHasDefaultValue()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var sortParam = method?.GetParameters().FirstOrDefault(p => p.Name == "sort");

        // Assert
        Assert.NotNull(sortParam);
        Assert.True(sortParam.HasDefaultValue);
        Assert.Equal("effective_date", sortParam.DefaultValue);
    }

    [Fact]
    public void ITreasuryExchangeRateClient_PageSizeParameterHasDefaultValue()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var pageSizeParam = method?.GetParameters().FirstOrDefault(p => p.Name == "pageSize");

        // Assert
        Assert.NotNull(pageSizeParam);
        Assert.True(pageSizeParam.HasDefaultValue);
        Assert.Equal(10000, pageSizeParam.DefaultValue);
    }

    [Fact]
    public void ITreasuryExchangeRateClient_AllParametersAreQueryParameters()
    {
        // Arrange
        var interfaceType = typeof(ITreasuryExchangeRateClient);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var parameters = method?.GetParameters();

        // Assert
        Assert.NotNull(parameters);
        foreach (var param in parameters)
        {
            var queryAttrs = param.GetCustomAttributes(typeof(QueryAttribute), false);
            Assert.NotEmpty(queryAttrs);
        }
    }

    [Fact]
    public void TreasuryRatesResponse_HasDataProperty()
    {
        // Arrange
        var responseType = typeof(TreasuryRatesResponse);

        // Act
        var dataProperty = responseType.GetProperty("Data");

        // Assert
        Assert.NotNull(dataProperty);
        Assert.True(dataProperty.PropertyType.IsGenericType &&
                    dataProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>),
            "Data property should be List<TreasuryRateData>");
    }

    [Fact]
    public void TreasuryRateData_HasRequiredProperties()
    {
        // Arrange
        var dataType = typeof(TreasuryRateData);
        var expectedProperties = new[] { "Country_Currency_Desc", "Exchange_Rate", "Record_Date", "Effective_Date" };

        // Act
        var actualProperties = dataType.GetProperties().Select(p => p.Name).ToArray();

        // Assert
        foreach (var expectedProp in expectedProperties)
        {
            Assert.Contains(expectedProp, actualProperties);
        }
    }

    [Fact]
    public void TreasuryRateData_ExchangeRateIsDecimal()
    {
        // Arrange
        var dataType = typeof(TreasuryRateData);

        // Act
        var exchangeRateProperty = dataType.GetProperty("Exchange_Rate");

        // Assert
        Assert.NotNull(exchangeRateProperty);
        Assert.Equal(typeof(decimal), exchangeRateProperty.PropertyType);
    }
}
