namespace Checknote.Modules.Users.Domain.Users;

using System;
using Checknote.Common.Domain;

public sealed class User : Entity<Guid>
{
    private User()
    {
        IdentityId = string.Empty;
        Name = string.Empty;
        Email = string.Empty;
    }

    private User(Guid id, string identityId, string name, string email)
        : base(id)
    {
        IdentityId = identityId;
        Name = name;
        Email = email;
    }

    public string IdentityId { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public static User Create(Guid id, string identityId, string name, string email)
    {
        return new User(id, identityId, name, email);
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
    }
}
