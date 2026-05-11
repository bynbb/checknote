namespace Checknote.Api.UnitTests.App.Authentication;

using System.Collections.Generic;
using System.Linq;
using Checknote.Api.Authentication;
using Checknote.Api.UnitTests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal static class JwtBearerConfigureOptionsTests
{
    public static void Run()
    {
        IConfiguration configuration = CreateConfiguration();
        JwtBearerConfigureOptions configureOptions = new(configuration);
        JwtBearerOptions jwtBearerOptions = new();

        configureOptions.Configure(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions);

        TestAssert.Equal("account", jwtBearerOptions.Audience, "JWT bearer audience");
        TestAssert.Equal(
            "http://127.0.0.1:8080/realms/checknote/.well-known/openid-configuration",
            jwtBearerOptions.MetadataAddress,
            "JWT bearer metadata address");
        TestAssert.Equal(false, jwtBearerOptions.RequireHttpsMetadata, "JWT bearer HTTPS metadata requirement");
        TestAssert.True(
            jwtBearerOptions.TokenValidationParameters.ValidIssuers?.Contains(
                "http://127.0.0.1:8080/realms/checknote") == true,
            "JWT bearer valid issuer should bind from configuration.");

        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddChecknoteAuthentication(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AuthenticationOptions authenticationOptions =
            serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        JwtBearerOptions configuredOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        TestAssert.Equal(
            JwtBearerDefaults.AuthenticationScheme,
            authenticationOptions.DefaultAuthenticateScheme,
            "Default authenticate scheme");
        TestAssert.Equal(
            JwtBearerDefaults.AuthenticationScheme,
            authenticationOptions.DefaultChallengeScheme,
            "Default challenge scheme");
        TestAssert.Equal("account", configuredOptions.Audience, "Configured JWT bearer audience");

        ChecknoteKeycloakOptions keycloakOptions =
            serviceProvider.GetRequiredService<IOptions<ChecknoteKeycloakOptions>>().Value;

        TestAssert.Equal("http://127.0.0.1:8080", keycloakOptions.AuthServerUrl, "Keycloak auth server URL");
        TestAssert.Equal("checknote", keycloakOptions.Realm, "Keycloak realm");
        TestAssert.Equal("checknote-angular", keycloakOptions.PublicClientId, "Keycloak public client id");
    }

    private static IConfiguration CreateConfiguration()
    {
        Dictionary<string, string?> values = new()
        {
            ["Authentication:Audience"] = "account",
            ["Authentication:MetadataAddress"] =
                "http://127.0.0.1:8080/realms/checknote/.well-known/openid-configuration",
            ["Authentication:RequireHttpsMetadata"] = "false",
            ["Authentication:TokenValidationParameters:ValidIssuers:0"] =
                "http://127.0.0.1:8080/realms/checknote",
            ["Keycloak:AuthServerUrl"] = "http://127.0.0.1:8080",
            ["Keycloak:Realm"] = "checknote",
            ["Keycloak:PublicClientId"] = "checknote-angular",
            ["Keycloak:HealthUrl"] = "http://127.0.0.1:9000/health/",
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
