using OrderGrid.Domain.Common;

namespace OrderGrid.Domain.ValueObjects;

public sealed record ShippingAddress
{
    public ShippingAddress(string recipient, string line1, string city, string postalCode,
        string countryCode, string? line2 = null, string? region = null)
    {
        Recipient = Required(recipient, nameof(recipient), 120);
        Line1 = Required(line1, nameof(line1), 160);
        City = Required(city, nameof(city), 100);
        PostalCode = Required(postalCode, nameof(postalCode), 24);
        CountryCode = Required(countryCode, nameof(countryCode), 2).ToUpperInvariant();
        Line2 = Optional(line2, 160);
        Region = Optional(region, 100);
    }

    public string Recipient { get; }
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string? Region { get; }
    public string PostalCode { get; }
    public string CountryCode { get; }

    private static string Required(string value, string name, int max)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is 0 || normalized.Length > max)
            throw new DomainException($"{name} is required and must be at most {max} characters.");
        return normalized;
    }

    private static string? Optional(string? value, int max)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized)) return null;
        if (normalized.Length > max) throw new DomainException($"Value must be at most {max} characters.");
        return normalized;
    }
}
