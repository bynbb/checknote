namespace Checknote.Api.Authentication;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddChecknoteAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthorization();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services.AddHttpContextAccessor();
        services.ConfigureOptions<JwtBearerConfigureOptions>();
        services.Configure<ChecknoteKeycloakOptions>(options =>
        {
            IConfigurationSection section =
                configuration.GetSection(ChecknoteKeycloakOptions.ConfigurationSectionName);

            options.Realm = section["Realm"] ?? string.Empty;
            options.PublicClientId = section["PublicClientId"] ?? string.Empty;
            options.HealthUrl = section["HealthUrl"] ?? string.Empty;
        });

        return services;
    }
}
