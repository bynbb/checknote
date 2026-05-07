namespace Checknote.Modules.Users.Domain.Users;

using Checknote.Common.Domain;

public sealed class User : Entity<string>
{
    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public User(string id, string name, string email)
        : base(id)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; private set; }

    public string Email { get; private set; }
}
