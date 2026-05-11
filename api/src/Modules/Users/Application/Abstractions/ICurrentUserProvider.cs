namespace Checknote.Modules.Users.Application.Abstractions;

using Checknote.Common.Domain;
using Checknote.Modules.Users.Domain.Users;

public interface ICurrentUserProvider
{
    Result<User> GetCurrentUser();
}
