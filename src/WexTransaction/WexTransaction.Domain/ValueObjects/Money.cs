namespace WexTransaction.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if (value <= 0)
            throw new InvalidAmountException("Amount must be a positive value.");

        Value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static implicit operator decimal(Money m) => m.Value;

    public override string ToString() => Value.ToString("F2");
}
