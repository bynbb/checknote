namespace Checknote.Modules.Users.Presentation.Users;

using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class GetCurrentUserEndpoint
{
    public const string Route = "/api/users/current";

    public static IEndpointRouteBuilder MapGetCurrentUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, (GetCurrentUserQueryHandler handler) =>
            Results.Ok(handler.Handle(new GetCurrentUserQuery())));

        return endpoints;
    }
}
