namespace Checknote.Api.Authentication;

public sealed class ChecknoteKeycloakOptions
{
    public const string ConfigurationSectionName = "Keycloak";

    public string Realm { get; set; } = string.Empty;

    public string PublicClientId { get; set; } = string.Empty;

    public string HealthUrl { get; set; } = string.Empty;
}
