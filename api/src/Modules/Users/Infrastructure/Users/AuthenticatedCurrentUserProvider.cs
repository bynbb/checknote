namespace Checknote.Modules.Users.Infrastructure.Users;

using System.Security.Claims;
using Checknote.Common.Domain;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Microsoft.AspNetCore.Http;

public sealed class AuthenticatedCurrentUserProvider : ICurrentUserProvider
{
    public const string SubjectClaimType = "sub";
    public const string PreferredUsernameClaimType = "preferred_username";
    public const string EmailClaimType = "email";
    public const string NameClaimType = "name";

    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthenticatedCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Result<AuthenticatedUser> GetCurrentUser()
    {
        ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Result.Failure<AuthenticatedUser>(GetCurrentUserErrors.AuthenticationRequired);
        }

        string? identityId = FindClaimValue(
            principal,
            SubjectClaimType,
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(identityId))
        {
            return Result.Failure<AuthenticatedUser>(GetCurrentUserErrors.MissingSubjectClaim);
        }

        string? email = FindClaimValue(
            principal,
            EmailClaimType,
            ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<AuthenticatedUser>(GetCurrentUserErrors.MissingEmailClaim);
        }

        string name = FindClaimValue(
            principal,
            PreferredUsernameClaimType,
            NameClaimType,
            ClaimTypes.Name) ?? email;

        return Result.Success(new AuthenticatedUser(identityId, name, email));
    }

    private static string? FindClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (string claimType in claimTypes)
        {
            string? value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
