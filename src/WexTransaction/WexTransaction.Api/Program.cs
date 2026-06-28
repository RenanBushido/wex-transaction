using WexTransaction.Api;
using WexTransaction.Api.Exceptions;
using WexTransaction.Application.Extensions;
using WexTransaction.Infra.Database.Extensions;
using WexTransaction.Infra.Services.RatesExchange.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddExternalApis(builder.Configuration)
    .AddApplicationServices();

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapTransactionEndpoints();

app.Run();
