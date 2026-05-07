namespace Checknote.Modules.Users.Application.Users.GetCurrentUser;

using Checknote.Common.Application.Messaging;
using Checknote.Modules.Users.Domain.Users;

public sealed record GetCurrentUserQuery : IQuery<User>;
