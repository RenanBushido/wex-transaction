namespace WexTransaction.Tests.Domain.Interfaces;

using Xunit;
using WexTransaction.Domain.Interfaces;
using WexTransaction.Domain.Common;

public class IBaseRepositoryTests
{
    [Fact]
    public void IBaseRepository_IsGenericInterface()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var isGeneric = interfaceType.IsGenericTypeDefinition;

        // Assert
        Assert.True(isGeneric, "IBaseRepository should be a generic interface");
    }

    [Fact]
    public void IBaseRepository_DefinesCreateMethod()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var method = interfaceType.GetMethod("Create");

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    [Fact]
    public void IBaseRepository_DefinesUpdateMethod()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var method = interfaceType.GetMethod("Update");

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    [Fact]
    public void IBaseRepository_DefinesDeleteMethod()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var method = interfaceType.GetMethod("Delete");

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    [Fact]
    public void IBaseRepository_DefinesGetMethod()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var method = interfaceType.GetMethod("Get");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>),
            "Get should return Task<T?>");
    }

    [Fact]
    public void IBaseRepository_DefinesGetAllMethod()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var method = interfaceType.GetMethod("GetAll");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType,
            "GetAll should return Task<List<T>>");
    }

    [Fact]
    public void IBaseRepository_GenericConstraintIsBaseEntity()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var genericArgs = interfaceType.GetGenericArguments();
        var constraints = genericArgs[0].GetGenericParameterConstraints();

        // Assert
        Assert.Single(genericArgs);
        Assert.Contains(typeof(BaseEntity), constraints);
    }

    [Fact]
    public void IBaseRepository_ContainsFiveRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);
        var expectedMethods = new[] { "Create", "Update", "Delete", "Get", "GetAll" };

        // Act
        var actualMethods = interfaceType.GetMethods()
            .Select(m => m.Name)
            .Distinct()
            .ToArray();

        // Assert
        foreach (var expectedMethod in expectedMethods)
        {
            Assert.Contains(expectedMethod, actualMethods);
        }
    }

    [Fact]
    public void IBaseRepository_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "IBaseRepository should be public");
    }

    [Fact]
    public void IBaseRepository_FollowsNamingConvention()
    {
        // Arrange
        var interfaceType = typeof(IBaseRepository<>);

        // Act
        var name = interfaceType.Name;

        // Assert
        Assert.True(name.StartsWith("I"), "Interface should start with 'I'");
    }
}
