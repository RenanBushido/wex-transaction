namespace WexTransaction.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(ErrorHandlingBehavior<,>));
        });

        // Event Publisher (Port implementation from Application layer)
        // Phase 2B: No-op implementation (events not persisted)
        // Phase 3: Replace with EventStorePublisher for persistence
        services.AddScoped<IEventPublisher, NoOpEventPublisher>();

        // Query Services (Port implementations from Application layer)
        services.AddScoped<ITransactionQueryService, TransactionQueryService>();

        // Legacy Use Cases (backward compatibility, to be deprecated in Phase 2C)
        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IQueryTransactionUseCase, GetTransactionUseCase>();

        return services;
    }
}
