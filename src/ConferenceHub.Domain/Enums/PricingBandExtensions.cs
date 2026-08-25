namespace ConferenceHub.Domain.Enums;

public static class PricingBandExtensions
{
    public static decimal GetModifier(this PricingBand band) => band switch
    {
        PricingBand.Standard => 1.00m,
        PricingBand.Evening => 0.80m,
        PricingBand.Morning => 0.90m,
        PricingBand.Peak => 1.15m,
        _ => throw new ArgumentOutOfRangeException(nameof(band), band, null)
    };
}
