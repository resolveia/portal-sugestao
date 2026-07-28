namespace PortalSugestao.Api.Auth;

public class MockAuthOptions
{
    public const string SectionName = "MockAuth";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 480;
}
