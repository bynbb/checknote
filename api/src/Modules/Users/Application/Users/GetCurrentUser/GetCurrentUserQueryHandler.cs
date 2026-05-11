namespace Checknote.Modules.Users.Application.Users.GetCurrentUser;

using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Application.Messaging;
using Checknote.Common.Domain;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Domain.Users;

public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, User>
{
    private readonly ICurrentUserProvider currentUserProvider;
    private readonly IUserRepository userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUserProvider currentUserProvider,
        IUserRepository userRepository)
    {
        this.currentUserProvider = currentUserProvider;
        this.userRepository = userRepository;
    }

    public async Task<Result<User>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> authenticatedUserResult = currentUserProvider.GetCurrentUser();

        if (authenticatedUserResult.IsFailure)
        {
            return Result.Failure<User>(authenticatedUserResult.Error);
        }

        AuthenticatedUser authenticatedUser = authenticatedUserResult.Value;
        User? user = await userRepository.GetByIdentityIdAsync(
            authenticatedUser.IdentityId,
            cancellationToken);
        User? emailOwner = await userRepository.GetByEmailAsync(
            authenticatedUser.Email,
            cancellationToken);

        if (emailOwner is not null && (user is null || emailOwner.Id != user.Id))
        {
            return Result.Failure<User>(GetCurrentUserErrors.EmailAlreadyAssigned);
        }

        if (user is null)
        {
            user = User.Create(
                System.Guid.NewGuid(),
                authenticatedUser.IdentityId,
                authenticatedUser.Name,
                authenticatedUser.Email);
            userRepository.Insert(user);
        }
        else
        {
            user.UpdateProfile(authenticatedUser.Name, authenticatedUser.Email);
        }

        await userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(user);
    }
}
