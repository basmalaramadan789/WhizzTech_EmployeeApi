namespace WhizzTech.EmployeeApi.Domain.ValueObjects;


public sealed record Money
{
    public long AmountMinor { get; init; }
    public string CurrencyCode { get; init; }

    private Money() { CurrencyCode = string.Empty; }

    public Money(long amountMinor, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("CurrencyCode must be a 3-letter ISO 4217 code.", nameof(currencyCode));
        if (amountMinor < 0)
            throw new ArgumentException("AmountMinor cannot be negative.", nameof(amountMinor));

        AmountMinor = amountMinor;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    public decimal ToMajorUnit() => AmountMinor / 100m;

    public override string ToString() => $"{ToMajorUnit():F2} {CurrencyCode}";
}
