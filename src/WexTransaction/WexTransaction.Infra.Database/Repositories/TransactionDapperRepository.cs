namespace WexTransaction.Infra.Database.Repositories;

public class TransactionDapperRepository(IDbConnection connection) : ITransactionDapperRepository
{
    #region Variables

    private readonly IDbConnection _connection = connection;

    #endregion


    #region Public Methods

    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
    {
        var query = "SELECT " + 
            "transaction_id as Id," +
            "transaction_description as Description," +
            "transaction_date as TransactionDate," +
            "transaction_amount::DECIMAL as Amount " +
            "FROM tb_purchase_transaction WHERE transaction_id = @transactionId::uuid";

        return await _connection.QueryFirstOrDefaultAsync<PurchaseTransaction>(query, new { transactionId = id.ToString() }) ?? null!;
    }

    #endregion
}
