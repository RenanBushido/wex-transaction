namespace WexTransaction.Infra.Database.Repositories;

public class TransactionDapperRepository(IDbConnection connection) : ITransactionDapperRepository
{
    #region Variables

    private readonly IDbConnection _connection = connection;

    #endregion


    #region Public Methods

    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
    {
        var query = "SELECT * FROM 'tb_purchase_transaction' WHERE 'transaction_id' = @transactionId";

        return await _connection.QueryFirstOrDefaultAsync<PurchaseTransaction>(query, new { transactionId = id }) ?? null!;
    }

    #endregion
}
