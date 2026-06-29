namespace WexTransaction.Domain.ValueObjects;

public readonly record struct ExchangeRate
{
    public string Country { get; }
    public string Currency { get; }
    public decimal Rate { get; }
    public DateTimeOffset EffectiveDate { get; }

    public ExchangeRate(string country, string currency, decimal rate, DateTimeOffset effectiveDate)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country must not be null or empty.", nameof(country));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency must not be null or empty.", nameof(currency));

        if (rate <= 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be a positive value.");

        // if (effectiveDate.Offset.Hours != 0)
        //     throw new ArgumentOutOfRangeException(nameof(effectiveDate), "Effective date must be UTC.");

        Country = country;
        Currency = currency;
        Rate = rate;
        EffectiveDate = effectiveDate;
    }
}
