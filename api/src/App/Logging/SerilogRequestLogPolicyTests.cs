namespace Checknote.Api.Logging.Tests;

using System;
using System.Collections.Generic;
using Checknote.Api.Logging;
using Microsoft.AspNetCore.Http;

internal static class Program
{
    public static int Main()
    {
        AssertEqual(
            ChecknoteRequestLogLevel.Information,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 200), 1.2, null),
            "2xx request level");
        AssertEqual(
            ChecknoteRequestLogLevel.Warning,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 404), 1.2, null),
            "4xx request level");
        AssertEqual(
            ChecknoteRequestLogLevel.Error,
            SerilogRequestLogPolicy.GetLevel(CreateContext("/api/todos", 500), 1.2, null),
            "5xx request level");
        AssertEqual(
            ChecknoteRequestLogLevel.Error,
            SerilogRequestLogPolicy.GetLevel(
                CreateContext("/api/todos", 200),
                1.2,
                new InvalidOperationException("boom")),
            "exception request level");

        AssertEqual("api", SerilogRequestLogPolicy.GetArea("/api/todos"), "API area");
        AssertEqual("api", SerilogRequestLogPolicy.GetArea("/hello-world"), "hello-world area");
        AssertEqual("health", SerilogRequestLogPolicy.GetArea("/health"), "health area");
        AssertEqual("static", SerilogRequestLogPolicy.GetArea("/main-ABC123.js"), "static area");
        AssertEqual("web", SerilogRequestLogPolicy.GetArea("/client/route"), "web area");
        AssertEqual("web", SerilogRequestLogPolicy.GetArea("/"), "root area");

        return 0;
    }

    private static DefaultHttpContext CreateContext(string path, int statusCode)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Response.StatusCode = statusCode;
        return context;
    }

    private static void AssertEqual<T>(T expected, T actual, string description)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{description}: expected {expected}, got {actual}");
        }
    }

}
