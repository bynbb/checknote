namespace Checknote.Api.UnitTests.Common.Presentation.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Checknote.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

internal static class EndpointRegistrationExtensionsTests
{
    public static async Task Run()
    {
        ServiceCollection services = new();
        services.AddEndpoints(typeof(EndpointRegistrationFirstEndpoint).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        Type[] endpointTypes = provider.GetRequiredService<IEnumerable<IEndpoint>>()
            .Select(endpoint => endpoint.GetType())
            .ToArray();

        TestAssert.True(
            endpointTypes.Contains(typeof(EndpointRegistrationFirstEndpoint)),
            "AddEndpoints should register the first discovered endpoint.");
        TestAssert.True(
            endpointTypes.Contains(typeof(EndpointRegistrationSecondEndpoint)),
            "AddEndpoints should register the second discovered endpoint.");

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpoints(typeof(EndpointRegistrationFirstEndpoint).Assembly);

        await using WebApplication app = builder.Build();
        app.MapEndpoints();

        IEndpointRouteBuilder routeBuilder = app;
        string[] routePatterns = routeBuilder.DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText ?? string.Empty)
            .ToArray();

        TestAssert.True(
            routePatterns.Contains(EndpointRegistrationFirstEndpoint.Route),
            "MapEndpoints should map the first registered endpoint.");
        TestAssert.True(
            routePatterns.Contains(EndpointRegistrationSecondEndpoint.Route),
            "MapEndpoints should map the second registered endpoint.");
    }
}

public sealed class EndpointRegistrationFirstEndpoint : IEndpoint
{
    public const string Route = "/test/endpoints/first";

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, () => "first");
    }
}

public sealed class EndpointRegistrationSecondEndpoint : IEndpoint
{
    public const string Route = "/test/endpoints/second";

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, () => "second");
    }
}
