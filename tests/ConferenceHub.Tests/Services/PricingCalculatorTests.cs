using ConferenceHub.Application.Services;
using FluentAssertions;

namespace ConferenceHub.Tests.Services;

public class PricingCalculatorTests
{
    private readonly PricingCalculator _sut = new();
    private const decimal PricePerHour = 1000m;

    private DateTime At(int hour) => new(2026, 1, 1, hour, 0, 0);

    // Bands
    [Theory]
    [InlineData(6, 900)]
    [InlineData(7, 900)]
    [InlineData(8, 900)]
    [InlineData(9, 1000)]
    [InlineData(11, 1000)]
    [InlineData(12, 1150)]
    [InlineData(13, 1150)]
    [InlineData(14, 1000)]
    [InlineData(17, 1000)]
    [InlineData(18, 800)]
    [InlineData(22, 800)]
    public void Calculate_SingleHour_AppliesBandModifier(int startHour, decimal expected)
    {
        var result = _sut.Calculate(PricePerHour, At(startHour), At(startHour + 1), []);
        result.Should().Be(expected);
    }

    [Fact]
    public void Calculate_CrossBand()
    {
        var result = _sut.Calculate(PricePerHour, At(10), At(14), []);
        result.Should().Be(4300m);
    }

    [Fact]
    public void Calculate_FullDate_AllBands()
    {
        var result = _sut.Calculate(PricePerHour, At(6), At(23), []);
        result.Should().Be(16000m);
    }

    // With services
    [Fact]
    public void Calculate_AddServices_ToRoomTotal()
    {
        var result = _sut.Calculate(PricePerHour, At(10), At(11), [500m, 300m]);
        result.Should().Be(1800m);
    }

    // Invalid input
    [Fact]
    public void Calculate_EndBeforeStart_Throws()
    {
        var act = () => _sut.Calculate(PricePerHour, At(14), At(10), []);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calculate_EndEqualsStart_Throws()
    {
        var act = () => _sut.Calculate(PricePerHour, At(10), At(10), []);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    public void Calculate_PartialHour_Throws(int minutes)
    {
        var start = new DateTime(2026, 1, 1, 10, minutes, 0);
        var end = At(12);
        var act = () => _sut.Calculate(PricePerHour, start, end, []);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void Calculate_NightHours_Throws(int nightHours)
    {
        var act = () => _sut.Calculate(PricePerHour, At(nightHours), At(nightHours + 1), []);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Calculate_TwentyThree_Throws()
    {
        var start = new DateTime(2026, 1, 1, 23, 0, 0);
        var end = new DateTime(2026, 1, 2, 0, 0, 0);
        var act = () => _sut.Calculate(PricePerHour, start, end, []);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
