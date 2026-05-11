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

    public GetCurrentUserQueryHandler(ICurrentUserProvider currentUserProvider)
    {
        this.currentUserProvider = currentUserProvider;
    }

    public Task<Result<User>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(currentUserProvider.GetCurrentUser());
    }
}
