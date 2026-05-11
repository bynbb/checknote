namespace Checknote.Modules.Users.Application.Abstractions;

public sealed record AuthenticatedUser(string IdentityId, string Name, string Email);
