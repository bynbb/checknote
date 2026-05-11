namespace Checknote.Modules.Users.Application.Users.GetCurrentUser;

using Checknote.Common.Domain;

public static class GetCurrentUserErrors
{
    public static readonly Error AuthenticationRequired = Error.Problem(
        "Users.AuthenticationRequired",
        "An authenticated user is required.");

    public static readonly Error MissingSubjectClaim = Error.Problem(
        "Users.MissingSubjectClaim",
        "The authenticated user is missing the Keycloak subject claim.");

    public static readonly Error MissingEmailClaim = Error.Problem(
        "Users.MissingEmailClaim",
        "The authenticated user is missing the email claim.");

    public static readonly Error EmailAlreadyAssigned = Error.Conflict(
        "Users.EmailAlreadyAssigned",
        "The authenticated user's email is already assigned to a different Checknote user.");
}
