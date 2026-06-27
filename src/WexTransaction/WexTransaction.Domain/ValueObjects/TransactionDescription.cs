namespace WexTransaction.Domain.ValueObjects;

public readonly record struct TransactionDescription
{
    private const int _maxLength = 50;

    public string Value { get; }

    public TransactionDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDescriptionException("Description must not be null or empty.");

        if (value.Length > _maxLength)
            throw new InvalidDescriptionException($"Description must not exceed {_maxLength} characters.");

        Value = value;
    }

    public static implicit operator string(TransactionDescription d) => d.Value;
    public static implicit operator TransactionDescription(string s) => new(s);

    public override string ToString() => Value;
}
