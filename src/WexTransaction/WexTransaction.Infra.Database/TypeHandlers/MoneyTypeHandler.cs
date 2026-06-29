namespace WexTransaction.Infra.Database.TypeHandlers;

public sealed class MoneyTypeHandler : SqlMapper.TypeHandler<Money>
{
    public override void SetValue(IDbDataParameter parameter, Money value)
        => parameter.Value = (decimal)value;

    public override Money Parse(object value)
        => new Money(Convert.ToDecimal(value));
}
