namespace Checknote.Tools.AuthSmoke;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static partial class Program
{
    public static int Main()
    {
        try
        {
            AuthSmokeOptions options = AuthSmokeOptions.FromEnvironment();
            UnlockChilkat(options.ChilkatLicenseKey);

            Console.WriteLine("Checknote authenticated API smoke started.");
            Console.WriteLine($"Keycloak realm: {options.Realm}");
            Console.WriteLine($"Keycloak client: {options.ClientId}");
            Console.WriteLine($"Current user endpoint: {options.CurrentUserUrl}");
            Console.WriteLine($"Todos endpoint: {options.TodosUrl}");

            DiscoveryDocument discoveryDocument = VerifyDiscovery(options);
            TokenSet tokenSet = FetchToken(options, discoveryDocument.TokenEndpoint);
            VerifyJwt(options, tokenSet.AccessToken);
            UserProjection firstUser = VerifyCurrentUser(options, tokenSet.AccessToken);
            UserProjection secondUser = VerifyCurrentUser(options, tokenSet.AccessToken);
            VerifyStableCurrentUser(firstUser, secondUser);
            VerifyProtectedTodos(options, tokenSet.AccessToken);
            VerifyRejectedRequest("unauthenticated current user", options.CurrentUserUrl, null, 401);
            VerifyRejectedRequest("invalid-token current user", options.CurrentUserUrl, "not-a-valid-token", 401);

            Console.WriteLine("Checknote authenticated API smoke passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Checknote authenticated API smoke failed: {exception.Message}");
            return 1;
        }
    }

    private static void UnlockChilkat(string licenseKey)
    {
        Chilkat.Global chilkat = new();
        if (!chilkat.UnlockBundle(licenseKey))
        {
            throw new InvalidOperationException("Chilkat unlock failed.");
        }

        Console.WriteLine("Verified Chilkat unlock.");
    }

    private static DiscoveryDocument VerifyDiscovery(AuthSmokeOptions options)
    {
        string discoveryUrl = $"{options.KeycloakBaseUrl}/realms/{options.Realm}/.well-known/openid-configuration";
        using JsonDocument json = GetJson(discoveryUrl);
        JsonElement root = json.RootElement;

        string issuer = GetRequiredString(root, "issuer", "discovery issuer");
        string tokenEndpoint = GetRequiredString(root, "token_endpoint", "discovery token endpoint");

        ExpectEqual(options.ExpectedIssuer, issuer, "discovery issuer");
        Expect(tokenEndpoint.StartsWith(options.ExpectedIssuer, StringComparison.Ordinal), "Token endpoint should belong to the expected issuer.");

        Console.WriteLine("Verified Keycloak discovery document.");
        return new DiscoveryDocument(issuer, tokenEndpoint);
    }

    private static TokenSet FetchToken(AuthSmokeOptions options, string tokenEndpoint)
    {
        Chilkat.HttpRequest request = new();
        request.AddParam("grant_type", "password");
        request.AddParam("client_id", options.ClientId);
        request.AddParam("username", options.Username);
        request.AddParam("password", options.Password);
        request.AddParam("scope", "openid email profile");

        if (!string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            request.AddParam("client_secret", options.ClientSecret);
        }

        using Chilkat.Http http = new();
        using Chilkat.HttpResponse? response = http.PostUrlEncoded(tokenEndpoint, request);
        if (response is null)
        {
            throw new InvalidOperationException("Keycloak token request did not return a response.");
        }

        if (response.StatusCode != 200)
        {
            throw new InvalidOperationException(
                $"Keycloak token request returned HTTP {response.StatusCode}: {RedactSensitiveJson(response.BodyStr)}");
        }

        using JsonDocument json = JsonDocument.Parse(response.BodyStr);
        JsonElement root = json.RootElement;
        string accessToken = GetRequiredString(root, "access_token", "token response access token");
        string tokenType = GetRequiredString(root, "token_type", "token response token type");
        ExpectEqual("Bearer", tokenType, "token response token type");

        Console.WriteLine("Verified Keycloak password-grant token issue.");
        return new TokenSet(accessToken);
    }

    private static void VerifyJwt(AuthSmokeOptions options, string accessToken)
    {
        Chilkat.Jwt jwt = new();
        using JsonDocument header = JsonDocument.Parse(jwt.GetHeader(accessToken));
        using JsonDocument payload = JsonDocument.Parse(jwt.GetPayload(accessToken));

        string algorithm = GetRequiredString(header.RootElement, "alg", "JWT algorithm");
        Expect(!string.Equals(algorithm, "none", StringComparison.OrdinalIgnoreCase), "JWT algorithm must not be none.");

        JsonElement root = payload.RootElement;
        string issuer = GetRequiredString(root, "iss", "JWT issuer");
        string subject = GetRequiredString(root, "sub", "JWT subject");
        long expiration = GetRequiredInt64(root, "exp", "JWT expiration");
        string? audience = TryGetStringOrArray(root, "aud");

        ExpectEqual(options.ExpectedIssuer, issuer, "JWT issuer");
        Expect(!string.IsNullOrWhiteSpace(subject), "JWT subject should be present.");
        Expect(expiration > DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "JWT expiration should be in the future.");
        if (!string.IsNullOrWhiteSpace(options.ExpectedAudience))
        {
            Expect(audience?.Contains(options.ExpectedAudience, StringComparison.Ordinal) == true, "JWT audience should include the expected audience.");
        }

        Console.WriteLine("Verified issued JWT shape.");
    }

    private static UserProjection VerifyCurrentUser(AuthSmokeOptions options, string accessToken)
    {
        using JsonDocument json = GetJson(options.CurrentUserUrl, accessToken);
        JsonElement root = json.RootElement;

        UserProjection user = new(
            GetRequiredString(root, "id", "current user id"),
            GetRequiredString(root, "name", "current user name"),
            GetRequiredString(root, "email", "current user email"));

        if (!string.IsNullOrWhiteSpace(options.ExpectedName))
        {
            ExpectEqual(options.ExpectedName, user.Name, "current user expected name");
        }

        if (!string.IsNullOrWhiteSpace(options.ExpectedEmail))
        {
            ExpectEqual(options.ExpectedEmail, user.Email, "current user expected email");
        }

        Console.WriteLine("Verified /api/users/current with bearer token.");
        return user;
    }

    private static void VerifyStableCurrentUser(UserProjection firstUser, UserProjection secondUser)
    {
        ExpectEqual(firstUser.Id, secondUser.Id, "stable current user id");
        ExpectEqual(firstUser.Name, secondUser.Name, "stable current user name");
        ExpectEqual(firstUser.Email, secondUser.Email, "stable current user email");

        Console.WriteLine("Verified repeated /api/users/current projection is stable.");
    }

    private static void VerifyProtectedTodos(AuthSmokeOptions options, string accessToken)
    {
        using Chilkat.HttpResponse response = GetResponse(options.TodosUrl, accessToken);
        if (response.StatusCode != 200)
        {
            throw new InvalidOperationException($"Expected authenticated todos request to return 200, got HTTP {response.StatusCode}.");
        }

        Console.WriteLine("Verified /api/todos with bearer token.");
    }

    private static void VerifyRejectedRequest(string name, string url, string? bearerToken, int expectedStatusCode)
    {
        using Chilkat.HttpResponse response = GetResponse(url, bearerToken);
        if (response.StatusCode != expectedStatusCode)
        {
            throw new InvalidOperationException($"Expected {name} to return HTTP {expectedStatusCode}, got HTTP {response.StatusCode}.");
        }

        Console.WriteLine($"Verified {name} rejection.");
    }

    private static JsonDocument GetJson(string url, string? bearerToken = null)
    {
        using Chilkat.HttpResponse response = GetResponse(url, bearerToken);
        if (response.StatusCode != 200)
        {
            throw new InvalidOperationException($"Expected {url} to return 200, got HTTP {response.StatusCode}: {RedactSensitiveJson(response.BodyStr)}");
        }

        return JsonDocument.Parse(response.BodyStr);
    }

    private static Chilkat.HttpResponse GetResponse(string url, string? bearerToken)
    {
        using Chilkat.Http http = new();
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            http.SetRequestHeader("Authorization", $"Bearer {bearerToken}");
        }

        Chilkat.HttpResponse? response = http.QuickGetObj(url);
        if (response is null)
        {
            throw new InvalidOperationException($"HTTP request did not return a response for {url}.");
        }

        return response;
    }

    private static string GetRequiredString(JsonElement element, string propertyName, string description)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"Missing {description}.");
        }

        string? value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing {description}.");
        }

        return value;
    }

    private static long GetRequiredInt64(JsonElement element, string propertyName, string description)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) || !property.TryGetInt64(out long value))
        {
            throw new InvalidOperationException($"Missing {description}.");
        }

        return value;
    }

    private static string? TryGetStringOrArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        if (property.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        List<string> values = [];
        foreach (JsonElement item in property.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                string? value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }
        }

        return string.Join(' ', values);
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void ExpectEqual(string expected, string actual, string description)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{description}: expected '{expected}', got '{actual}'.");
        }
    }

    private static string RedactSensitiveJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        string redacted = SensitiveJsonPropertyRegex().Replace(body, "$1\"[redacted]\"");
        return redacted.Length <= 500 ? redacted : string.Concat(redacted.AsSpan(0, 500), "...");
    }

    [GeneratedRegex("(\"(?:access_token|refresh_token|id_token)\"\\s*:\\s*)\"[^\"]+\"", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveJsonPropertyRegex();
}

internal sealed record DiscoveryDocument(string Issuer, string TokenEndpoint);

internal sealed record TokenSet(string AccessToken);

internal sealed record UserProjection(string Id, string Name, string Email);

internal sealed record AuthSmokeOptions(
    string ChilkatLicenseKey,
    string Username,
    string Password,
    string KeycloakBaseUrl,
    string Realm,
    string ClientId,
    string? ClientSecret,
    string CurrentUserUrl,
    string TodosUrl,
    string? ExpectedAudience,
    string? ExpectedName,
    string? ExpectedEmail)
{
    private const string LicenseKeyVariable = "CHECKNOTE_CHILKAT_LICENSE_KEY";
    private const string UsernameVariable = "CHECKNOTE_AUTH_SMOKE_USERNAME";
    private const string PasswordVariable = "CHECKNOTE_AUTH_SMOKE_PASSWORD";

    public string ExpectedIssuer => $"{KeycloakBaseUrl}/realms/{Realm}";

    public static AuthSmokeOptions FromEnvironment()
    {
        string keycloakBaseUrl = GetOptional("CHECKNOTE_AUTH_SMOKE_KEYCLOAK_BASE_URL", "https://auth.checknote.io").TrimEnd('/');

        return new AuthSmokeOptions(
            GetRequired(LicenseKeyVariable),
            GetRequired(UsernameVariable),
            GetRequired(PasswordVariable),
            keycloakBaseUrl,
            GetOptional("CHECKNOTE_AUTH_SMOKE_REALM", "checknote"),
            GetOptional("CHECKNOTE_AUTH_SMOKE_CLIENT_ID", "checknote-angular"),
            GetOptionalOrNull("CHECKNOTE_AUTH_SMOKE_CLIENT_SECRET"),
            GetOptional("CHECKNOTE_AUTH_SMOKE_CURRENT_USER_URL", "https://www.checknote.io/api/users/current"),
            GetOptional("CHECKNOTE_AUTH_SMOKE_TODOS_URL", "https://www.checknote.io/api/todos"),
            GetOptionalOrNull("CHECKNOTE_AUTH_SMOKE_EXPECTED_AUDIENCE"),
            GetOptionalOrNull("CHECKNOTE_AUTH_SMOKE_EXPECTED_NAME"),
            GetOptionalOrNull("CHECKNOTE_AUTH_SMOKE_EXPECTED_EMAIL"));
    }

    private static string GetRequired(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} must be configured.");
        }

        return value;
    }

    private static string GetOptional(string name, string defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static string? GetOptionalOrNull(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
