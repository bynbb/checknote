namespace Checknote.Modules.Users.Application.Users.GetCurrentUser;

using Checknote.Common.Application.Messaging;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Domain.Users;

public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, User>
{
    private readonly IUserRepository userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public User Handle(GetCurrentUserQuery query)
    {
        return userRepository.GetCurrentUser();
    }
}
