var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddExternalApis(builder.Configuration);
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

app.UseExceptionHandler();
app.UseApiCors();
app.UseHttpsRedirection();
app.MapTransactionEndpoints();

app.Run();
