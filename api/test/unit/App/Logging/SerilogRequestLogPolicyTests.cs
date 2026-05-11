namespace Checknote.Api.Logging.Tests;

using System;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.App.Authentication;
using Checknote.Api.UnitTests.Common.Presentation.Endpoints;
using Checknote.Api.UnitTests.Modules.Database;
using Checknote.Api.UnitTests.Modules.Todos.Application;
using Checknote.Api.UnitTests.Support;
using Checknote.Api.Logging;
using Microsoft.AspNetCore.Http;

internal static class Program
{
    public static async Task<int> Main()
    {
        TestAssert.Equal(
            ChecknoteRequestLogLevel.Information,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 200), 1.2, null),
            "2xx request level");
        TestAssert.Equal(
            ChecknoteRequestLogLevel.Warning,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 404), 1.2, null),
            "4xx request level");
        TestAssert.Equal(
            ChecknoteRequestLogLevel.Error,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 500), 1.2, null),
            "5xx request level");
        TestAssert.Equal(
            ChecknoteRequestLogLevel.Error,
            SerilogRequestLogPolicy.GetLevel(
                CreateContext("/api/todos", 200),
                1.2,
                new InvalidOperationException("boom")),
            "exception request level");

        TestAssert.Equal("api", SerilogRequestLogPolicy.GetArea("/api/todos"), "API area");
        TestAssert.Equal("api", SerilogRequestLogPolicy.GetArea("/hello-world"), "hello-world area");
        TestAssert.Equal("health", SerilogRequestLogPolicy.GetArea("/health"), "health area");
        TestAssert.Equal("static", SerilogRequestLogPolicy.GetArea("/main-ABC123.js"), "static area");
        TestAssert.Equal("web", SerilogRequestLogPolicy.GetArea("/client/route"), "web area");
        TestAssert.Equal("web", SerilogRequestLogPolicy.GetArea("/"), "root area");

        await MediatRDispatchTests.Run();
        JwtBearerConfigureOptionsTests.Run();
        await EndpointRegistrationExtensionsTests.Run();
        await SchemaBoundaryTests.Run();

        return 0;
    }

    private static DefaultHttpContext CreateContext(string path, int statusCode)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Response.StatusCode = statusCode;
        return context;
    }
}
