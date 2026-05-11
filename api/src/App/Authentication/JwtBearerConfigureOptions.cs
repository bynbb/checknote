namespace Checknote.Api.Authentication;

using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

public sealed class JwtBearerConfigureOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    public const string ConfigurationSectionName = "Authentication";

    private readonly IConfiguration configuration;

    public JwtBearerConfigureOptions(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Configure(JwtBearerOptions options)
    {
        IConfigurationSection section = configuration.GetSection(ConfigurationSectionName);

        options.Audience = section["Audience"];
        options.MetadataAddress = section["MetadataAddress"] ?? string.Empty;
        options.RequireHttpsMetadata =
            !bool.TryParse(section["RequireHttpsMetadata"], out bool requireHttpsMetadata) ||
            requireHttpsMetadata;

        string[] validIssuers = section
            .GetSection("TokenValidationParameters:ValidIssuers")
            .GetChildren()
            .Select(issuer => issuer.Value)
            .Where(issuer => !string.IsNullOrWhiteSpace(issuer))
            .Select(issuer => issuer!)
            .ToArray();

        if (validIssuers.Length > 0)
        {
            options.TokenValidationParameters.ValidIssuers = validIssuers;
        }
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
