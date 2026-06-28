namespace WexTransaction.Tests.Domain.Interfaces;
public class IUnitOfWorkTests
{
    [Fact]
    public void IUnitOfWork_DefinesCommitMethod()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var method = interfaceType.GetMethod("Commit");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ReturnType == typeof(System.Threading.Tasks.Task),
            "Commit should return Task");
    }

    [Fact]
    public void IUnitOfWork_CommitMethodAcceptsCancellationToken()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var method = interfaceType.GetMethod("Commit");
        var parameters = method?.GetParameters();

        // Assert
        Assert.NotNull(parameters);
        Assert.Single(parameters);
        Assert.Equal(typeof(System.Threading.CancellationToken), parameters[0].ParameterType);
    }

    [Fact]
    public void IUnitOfWork_HasNoOtherPublicMethods()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var methods = interfaceType.GetMethods()
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            .ToArray();

        // Assert
        Assert.Single(methods); // Only Commit method
    }

    [Fact]
    public void IUnitOfWork_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic, "IUnitOfWork should be public");
    }

    [Fact]
    public void IUnitOfWork_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface, "IUnitOfWork should be an interface");
    }

    [Fact]
    public void IUnitOfWork_FollowsNamingConvention()
    {
        // Arrange
        var interfaceType = typeof(IUnitOfWork);

        // Act
        var name = interfaceType.Name;

        // Assert
        Assert.True(name.StartsWith("I"), "Interface should start with 'I'");
    }
}
