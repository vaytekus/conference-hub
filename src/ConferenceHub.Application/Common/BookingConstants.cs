namespace ConferenceHub.Application.Common;

public static class BookingConstants
{
    public const int OpeningHour = 6;
    public const int ClosingHour = 23;

    public static bool IsWholeHour(DateTime time)
        => time.Minute == 0 && time.Second == 0 && time.Millisecond == 0;
}
