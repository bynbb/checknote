namespace Checknote.Modules.Users.Infrastructure.Users;

using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Domain.Users;

public sealed class InMemoryUserRepository : IUserRepository
{
    private static readonly User CurrentUser = new("user-1", "Ada Lovelace", "ada@checknote.local");

    public User GetCurrentUser()
    {
        return CurrentUser;
    }
}
