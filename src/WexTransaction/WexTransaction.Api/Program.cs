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
    builder.Services.AddSerilogLogging(builder.Configuration);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();

    var app = builder.Build();

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
    app.UseHttpsRedirection();
    app.MapTransactionEndpoints();

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
