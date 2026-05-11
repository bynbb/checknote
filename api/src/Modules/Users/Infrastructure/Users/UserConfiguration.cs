namespace Checknote.Modules.Users.Infrastructure.Users;

using Checknote.Modules.Users.Domain.Users;
using Checknote.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", Schemas.Users);

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.IdentityId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(user => user.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.IdentityId)
            .IsUnique();
    }
}
