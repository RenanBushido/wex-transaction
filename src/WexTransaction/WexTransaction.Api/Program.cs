using WexTransaction.Infra.Database.Extensions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting WexTransaction.API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApplicationServices();
    builder.Services.AddApiCors(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddExternalApis(builder.Configuration);
    builder.Services.AddApplicationHealthChecks(builder.Configuration);

    var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting")
        ?? throw new InvalidOperationException("Configuration section 'RateLimiting' not found");
    var policies = rateLimitingConfig.GetSection("Policies");

    builder.Services.AddRateLimiter(options =>
    {
        foreach (var policy in policies.GetChildren())
        {
            var permitLimit = policy.GetValue<int>("PermitLimit");
            var windowSeconds = policy.GetValue<int>("WindowSeconds");

            options.AddSlidingWindowLimiter(policy.Key, slidingWindowOptions =>
            {
                slidingWindowOptions.PermitLimit = permitLimit;
                slidingWindowOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                slidingWindowOptions.SegmentsPerWindow = 8;
            });
        }

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.HttpContext.Response.WriteAsync(
                "Rate limit exceeded. Too many requests.", cancellationToken);
        };
    });

    builder.Services.AddSerilogLogging(builder.Configuration);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Ensure database directory exists with proper permissions (755) for Docker volume mounts.
    // This must happen before migrations execute.
    // app.EnsureDatabaseDirectory();

    // Apply pending EF Core migrations before accepting requests.
    // This ensures the database schema is initialized and up-to-date.
    // If migrations fail, the application will not start.
    app.MigrateDatabase();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/openapi/v1.json", "WexTransaction")
        );
    }

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseApiCors();
    app.UseRateLimiter();
    app.UseHttpsRedirection();
    app.MapTransactionEndpoints();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
