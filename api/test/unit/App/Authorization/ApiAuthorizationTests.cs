namespace Checknote.Api.UnitTests.App.Authorization;

using System;
using System.Linq;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

internal static class ApiAuthorizationTests
{
    public static async Task Run()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        await using WebApplication app = ChecknoteApi.Build(builder);

        AssertRequiresAuthorization(app, "/api/users/current", "GET");
        AssertRequiresAuthorization(app, "/api/todos", "GET");
        AssertRequiresAuthorization(app, "/api/todos/task-list", "PUT");
    }

    private static void AssertRequiresAuthorization(
        WebApplication app,
        string routePattern,
        string method)
    {
        RouteEndpoint endpoint = FindEndpoint(app, routePattern, method);

        bool hasAuthorization = endpoint.Metadata
            .OfType<IAuthorizeData>()
            .Any();

        TestAssert.True(
            hasAuthorization,
            $"{method} {routePattern} should require authorization.");
    }

    private static RouteEndpoint FindEndpoint(
        WebApplication app,
        string routePattern,
        string method)
    {
        RouteEndpoint? endpoint = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .SingleOrDefault(endpoint =>
                endpoint.RoutePattern.RawText == routePattern &&
                endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.OrdinalIgnoreCase) == true);

        return endpoint ?? throw new InvalidOperationException(
            $"Expected endpoint {method} {routePattern} to be mapped.");
    }
}
