namespace Checknote.Modules.Users.Infrastructure.Users;

using System.Threading;
using System.Threading.Tasks;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Domain.Users;
using Checknote.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public sealed class SqlUserRepository : IUserRepository
{
    private readonly UsersDbContext dbContext;

    public SqlUserRepository(UsersDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.IdentityId == identityId,
            cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);
    }

    public void Insert(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
