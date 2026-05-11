namespace Checknote.Modules.Users.Application.Abstractions;

using System.Threading;
using System.Threading.Tasks;
using Checknote.Modules.Users.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    void Insert(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
