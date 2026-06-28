namespace WexTransaction.Tests.Domain.Interfaces;

using System.Collections.Generic;
using Xunit;
using WexTransaction.Domain.Interfaces;

public class IExchangeRateProviderTests
{
    [Fact]
    public void IExchangeRateProvider_DefinesGetExchangeRatesAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>),
            "GetExchangeRatesAsync should return Task<IEnumerable<ExchangeRate>>");
    }

    [Fact]
    public void IExchangeRateProvider_GetExchangeRatesAsyncAcceptsCountryParameter()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var parameters = method?.GetParameters();

        // Assert
        Assert.NotNull(parameters);
        Assert.True(parameters.Length >= 1, "Method should have at least one parameter");
        Assert.Equal("country", parameters[0].Name);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
    }

    [Fact]
    public void IExchangeRateProvider_GetExchangeRatesAsyncAcceptsCurrencyParameter()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var parameters = method?.GetParameters();

        // Assert
        Assert.NotNull(parameters);
        Assert.True(parameters.Length >= 2, "Method should have at least two parameters");
        Assert.Equal("currency", parameters[1].Name);
        Assert.Equal(typeof(string), parameters[1].ParameterType);
    }

    [Fact]
    public void IExchangeRateProvider_HasSinglePublicMethod()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var methods = interfaceType.GetMethods()
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            .ToArray();

        // Assert
        Assert.Single(methods); // Only GetExchangeRatesAsync
    }

    [Fact]
    public void IExchangeRateProvider_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "IExchangeRateProvider should be public");
    }

    [Fact]
    public void IExchangeRateProvider_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "IExchangeRateProvider should be an interface");
    }

    [Fact]
    public void IExchangeRateProvider_FollowsNamingConvention()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var name = interfaceType.Name;

        // Assert
        Assert.True(name.StartsWith("I"), "Interface should start with 'I'");
    }

    [Fact]
    public void IExchangeRateProvider_ReturnTypeIsEnumerableOfExchangeRate()
    {
        // Arrange
        var interfaceType = typeof(IExchangeRateProvider);

        // Act
        var method = interfaceType.GetMethod("GetExchangeRatesAsync");
        var returnType = method?.ReturnType;

        // Assert
        Assert.NotNull(returnType);
        Assert.True(returnType.IsGenericType, "Return type should be generic");
        var genericType = returnType.GetGenericArguments()[0];
        Assert.True(genericType.IsGenericType, "Generic argument should be IEnumerable<T>");
    }
}
