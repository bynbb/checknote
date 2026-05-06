namespace Checknote.Modules.Users.Application.Abstractions;

using Checknote.Modules.Users.Domain.Users;

public interface IUserRepository
{
    User GetCurrentUser();
}
