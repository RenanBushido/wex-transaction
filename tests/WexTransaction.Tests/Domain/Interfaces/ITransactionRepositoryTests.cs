namespace WexTransaction.Tests.Domain.Interfaces;

public class ITransactionRepositoryTests
{
    [Fact]
    public void ITransactionRepository_InheritsFromIBaseRepository()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransactionRepository);
        var baseRepositoryInterface = typeof(IBaseRepository<PurchaseTransaction>);

        // Assert
        Assert.True(interfaceType.GetInterfaces().Contains(baseRepositoryInterface),
            "ITransactionRepository should inherit from IBaseRepository<PurchaseTransaction>");
    }

    [Fact]
    public void ITransactionRepository_DefinesSavePurchaseTransactionMethod()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);

        // Act
        var method = interfaceType.GetMethod("SavePurchaseTransaction");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType == typeof(System.Threading.Tasks.Task),
            "SavePurchaseTransaction should return Task");
    }

    [Fact]
    public void ITransactionRepository_DefinesGetByIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);

        // Act
        var method = interfaceType.GetMethod("GetByIdAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>),
            "GetByIdAsync should return Task<T?>");
    }

    [Fact]
    public void ITransactionRepository_ContainsRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);
        var expectedMethods = new[] { "SavePurchaseTransaction", "GetByIdAsync" };

        // Act
        var actualMethods = interfaceType.GetMethods()
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            .Select(m => m.Name)
            .ToArray();

        // Assert
        foreach (var expectedMethod in expectedMethods)
        {
            Assert.True(actualMethods.Contains(expectedMethod),
                $"Method {expectedMethod} should be in interface");
        }
    }

    [Fact]
    public void ITransactionRepository_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "ITransactionRepository should be public");
    }

    [Fact]
    public void ITransactionRepository_InheritsFromIBaseRepository_Verified()
    {
        // Arrange
        var interfaceType = typeof(ITransactionRepository);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Contains(typeof(IBaseRepository<PurchaseTransaction>), baseInterfaces);
    }
}
