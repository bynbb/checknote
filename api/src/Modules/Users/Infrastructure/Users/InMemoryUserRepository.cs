namespace Checknote.Modules.Users.Infrastructure.Users;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Domain.Users;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly object gate = new();
    private readonly List<User> users = [];

    public Task<User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            return Task.FromResult(users.SingleOrDefault(user => user.IdentityId == identityId));
        }
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            return Task.FromResult(users.SingleOrDefault(
                user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public void Insert(User user)
    {
        lock (gate)
        {
            users.Add(user);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
