namespace ConferenceHub.Application.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int AccessTokenExpiresInMinutes { get; set; }
}
