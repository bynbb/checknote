namespace Checknote.Modules.Users.Presentation.Users;

using System.Threading;
using Checknote.Common.Domain;
using Checknote.Common.Presentation.Endpoints;
using Checknote.Common.Presentation.Results;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public const string Route = "/api/users/current";

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, async (ISender sender, CancellationToken cancellationToken) =>
        {
            Result<User> result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);

            return result.Match<User, IResult>(
                Results.Ok,
                ApiResults.Problem);
        });
    }
}
