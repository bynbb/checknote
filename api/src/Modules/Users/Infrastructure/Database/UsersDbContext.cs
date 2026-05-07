namespace Checknote.Modules.Users.Infrastructure.Database;

using Checknote.Modules.Users.Domain.Users;
using Checknote.Modules.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Users);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
