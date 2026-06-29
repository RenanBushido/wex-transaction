namespace WexTransaction.Infra.Database.TypeHandlers;

public sealed class TransactionDescriptionTypeHandler : SqlMapper.TypeHandler<TransactionDescription>
{
    public override void SetValue(IDbDataParameter parameter, TransactionDescription value)
        => parameter.Value = (string)value;

    public override TransactionDescription Parse(object value)
        => new TransactionDescription(value.ToString()!);
}
