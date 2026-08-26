namespace ConferenceHub.Application.Options;

public class RefreshTokenSettings
{
    public const string SectionName = "RefreshToken";

    public int ExpiresInDays { get; set; } = 7;
    public int TokenSizeBytes { get; set; } = 64;
}
