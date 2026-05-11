namespace Checknote.Modules.Users.Application.Abstractions;

using Checknote.Common.Domain;

public interface ICurrentUserProvider
{
    Result<AuthenticatedUser> GetCurrentUser();
}
