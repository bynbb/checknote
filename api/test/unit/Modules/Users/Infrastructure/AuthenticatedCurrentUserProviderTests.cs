namespace Checknote.Api.UnitTests.Modules.Users.Infrastructure;

using System.Security.Claims;
using Checknote.Api.UnitTests.Support;
using Checknote.Common.Domain;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Infrastructure.Users;
using Microsoft.AspNetCore.Http;

internal static class AuthenticatedCurrentUserProviderTests
{
    public static void Run()
    {
        MapsKeycloakClaimsToTheCurrentUser();
        FallsBackToNameClaimWhenPreferredUsernameIsMissing();
        RejectsUnauthenticatedRequests();
        RejectsMissingSubjectClaims();
        RejectsMissingEmailClaims();
    }

    private static void MapsKeycloakClaimsToTheCurrentUser()
    {
        Result<AuthenticatedUser> result = GetCurrentUser(
            new Claim("sub", "keycloak-user-id"),
            new Claim("preferred_username", "checknote-handle"),
            new Claim("name", "Real Name"),
            new Claim("email", "user@example.test"));

        TestAssert.True(result.IsSuccess, "Authenticated user result should succeed.");
        TestAssert.Equal("keycloak-user-id", result.Value.IdentityId, "Authenticated user identity id");
        TestAssert.Equal("checknote-handle", result.Value.Name, "Authenticated user display name");
        TestAssert.Equal("user@example.test", result.Value.Email, "Authenticated user email");
    }

    private static void FallsBackToNameClaimWhenPreferredUsernameIsMissing()
    {
        Result<AuthenticatedUser> result = GetCurrentUser(
            new Claim("sub", "keycloak-user-id"),
            new Claim("name", "Fallback Name"),
            new Claim("email", "user@example.test"));

        TestAssert.True(result.IsSuccess, "Authenticated user result should succeed.");
        TestAssert.Equal("Fallback Name", result.Value.Name, "Authenticated user fallback display name");
    }

    private static void RejectsUnauthenticatedRequests()
    {
        DefaultHttpContext context = new();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        Result<AuthenticatedUser> result = new AuthenticatedCurrentUserProvider(new HttpContextAccessor
        {
            HttpContext = context,
        }).GetCurrentUser();

        TestAssert.True(result.IsFailure, "Unauthenticated current user should fail.");
        TestAssert.Equal(
            GetCurrentUserErrors.AuthenticationRequired.Code,
            result.Error.Code,
            "Unauthenticated current user error code");
    }

    private static void RejectsMissingSubjectClaims()
    {
        Result<AuthenticatedUser> result = GetCurrentUser(
            new Claim("preferred_username", "checknote-handle"),
            new Claim("email", "user@example.test"));

        TestAssert.True(result.IsFailure, "Current user should require the Keycloak subject claim.");
        TestAssert.Equal(
            GetCurrentUserErrors.MissingSubjectClaim.Code,
            result.Error.Code,
            "Missing subject claim error code");
    }

    private static void RejectsMissingEmailClaims()
    {
        Result<AuthenticatedUser> result = GetCurrentUser(
            new Claim("sub", "keycloak-user-id"),
            new Claim("preferred_username", "checknote-handle"));

        TestAssert.True(result.IsFailure, "Current user should require the email claim.");
        TestAssert.Equal(
            GetCurrentUserErrors.MissingEmailClaim.Code,
            result.Error.Code,
            "Missing email claim error code");
    }

    private static Result<AuthenticatedUser> GetCurrentUser(params Claim[] claims)
    {
        DefaultHttpContext context = new();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        return new AuthenticatedCurrentUserProvider(new HttpContextAccessor
        {
            HttpContext = context,
        }).GetCurrentUser();
    }
}
