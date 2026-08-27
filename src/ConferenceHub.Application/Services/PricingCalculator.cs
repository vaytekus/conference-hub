using ConferenceHub.Application.Interfaces;
using ConferenceHub.Domain.Enums;

namespace ConferenceHub.Application.Services;

public class PricingCalculator : IPricingCalculator
{
    public decimal Calculate(
        decimal pricePerHour,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<decimal> servicePrices)
    {
        EnsureValidRange(startTime, endTime);
        EnsureWholeHour(startTime, endTime);

        var roomTotal = 0m;
        for (var cursor = startTime; cursor < endTime; cursor = cursor.AddHours(1))
        {
            var band = ResolveBand(cursor.Hour);
            roomTotal += pricePerHour * band.GetModifier();
        }

        return roomTotal + servicePrices.Sum();
    }

    private static PricingBand ResolveBand(int hourOfDay) => hourOfDay switch
    {
        >= 6 and < 9 => PricingBand.Morning,
        >= 12 and < 14 => PricingBand.Peak,
        >= 9 and < 18 => PricingBand.Standard,
        >= 18 and < 23 => PricingBand.Evening,
        _ => throw new ArgumentOutOfRangeException(nameof(hourOfDay), $"Hour {hourOfDay} is outside supported booking window (06:00–23:00).")
    };

    private static void EnsureValidRange(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("End time must be after start time.", nameof(end));
        }
    }

    private static void EnsureWholeHour(DateTime start, DateTime end)
    {
        if (!IsWholeHour(start) || !IsWholeHour(end))
        {
            throw new ArgumentException("Booking must start and end on whole hours.");
        }
    }

    private static bool IsWholeHour(DateTime dateTime)
    {
        return dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }
}
