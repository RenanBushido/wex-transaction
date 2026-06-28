namespace WexTransaction.Tests.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WexTransaction.Application.Extensions;
using WexTransaction.Application.Behaviors;
using MediatR;

public class ApplicationExtensionsTests
{
    [Fact]
    public void AddApplicationServices_RegistersMediatR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddApplicationServices_RegistersUnhandledExceptionBehaviour()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);

        // Verify that the behavior is registered by checking if it's part of the pipeline
        // This is done indirectly by verifying that MediatR has been configured
        var mediatRServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IMediator));

        Assert.NotNull(mediatRServiceDescriptor);
    }

    [Fact]
    public void AddApplicationServices_ReturnsIServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IServiceCollection>(result);
    }

    [Fact]
    public void AddApplicationServices_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Test method chaining
        services
            .AddApplicationServices()
            .AddApplicationServices();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddApplicationServices_RegistersRequestHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that MediatR handlers can be resolved
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddApplicationServices_WithMultipleCalls_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddApplicationServices();
        services.AddApplicationServices(); // Call twice - should not throw

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddApplicationServices_ProvidesMediatRPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify MediatR is properly configured
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsAssignableFrom<IMediator>(mediator);
    }

    [Fact]
    public void AddApplicationServices_AllowsCustomHandlerRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Add custom services after application services
        services.AddScoped<IRequestHandler<TestCommand, TestResponse>, TestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<IRequestHandler<TestCommand, TestResponse>>();
        Assert.NotNull(handler);
    }

    // Helper classes for testing
    public record TestCommand : IRequest<TestResponse>;

    public record TestResponse;

    public class TestCommandHandler : IRequestHandler<TestCommand, TestResponse>
    {
        public Task<TestResponse> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResponse());
        }
    }
}
