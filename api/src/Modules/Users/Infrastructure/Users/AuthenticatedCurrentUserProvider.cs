namespace Checknote.Modules.Users.Infrastructure.Users;

using System.Security.Claims;
using Checknote.Common.Domain;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Domain.Users;
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

    public Result<User> GetCurrentUser()
    {
        ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Result.Failure<User>(GetCurrentUserErrors.AuthenticationRequired);
        }

        string? id = FindClaimValue(
            principal,
            SubjectClaimType,
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Failure<User>(GetCurrentUserErrors.MissingSubjectClaim);
        }

        string? email = FindClaimValue(
            principal,
            EmailClaimType,
            ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(GetCurrentUserErrors.MissingEmailClaim);
        }

        string name = FindClaimValue(
            principal,
            PreferredUsernameClaimType,
            NameClaimType,
            ClaimTypes.Name) ?? email;

        return Result.Success(new User(id, name, email));
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
