namespace ConferenceHub.Application.Interfaces;

public interface IPricingCalculator
{
    decimal Calculate(
        decimal pricePerHour,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<decimal> servicePrices);
}
