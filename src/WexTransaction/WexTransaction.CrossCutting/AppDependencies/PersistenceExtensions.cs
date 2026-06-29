using System.Data;
using Npgsql;
using WexTransaction.Domain.Services;

namespace WexTransaction.CrossCutting.AppDependencies;

public static class PersistenceExtensions
{   
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WexTransactionConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<WexTransactionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<IDbConnection>(provider =>
        {
            var connecton = new NpgsqlConnection(connectionString);

            if(connecton.State != ConnectionState.Open) connecton.Open();

            return connecton;
        });

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionDapperRepository, TransactionDapperRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

}
