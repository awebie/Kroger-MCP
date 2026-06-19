namespace KrogerMcp.Infrastructure.Kroger.Options;

public sealed class KrogerApiOptions
{
    public const string SectionName = "Kroger";

    public string BaseUrl { get; set; } = "https://api.kroger.com";

    public string AuthorizationUrl { get; set; } = "https://api.kroger.com/v1/connect/oauth2/authorize";

    public string TokenUrl { get; set; } = "https://api.kroger.com/v1/connect/oauth2/token";

    public string? DefaultLocationId { get; set; }

    public string OAuthRedirectUri { get; set; } = "http://127.0.0.1:53682/callback";
}

public sealed class KrogerOAuthOptions
{
    public const string SectionName = "Kroger:OAuth";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? CustomerAccessToken { get; set; }
}
