namespace Checknote.Modules.Users.Domain.Users;

using Checknote.Common.Domain;

public sealed class User : Entity<string>
{
    public User(string id, string name, string email)
        : base(id)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; }

    public string Email { get; }
}
