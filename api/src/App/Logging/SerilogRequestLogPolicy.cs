namespace Checknote.Api.Logging;

using System;
using System.IO;
using Microsoft.AspNetCore.Http;

public static class SerilogRequestLogPolicy
{
    public const string SeqServerUrlVariable = "CHECKNOTE_SEQ_SERVER_URL";
    public const string SeqApiKeyVariable = "CHECKNOTE_SEQ_API_KEY";
    public const string MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    private const string HelloWorldRoute = "/hello-world";

    public static ChecknoteRequestLogLevel GetLevel(HttpContext httpContext, double elapsed, Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return ChecknoteRequestLogLevel.Error;
        }

        if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return ChecknoteRequestLogLevel.Warning;
        }

        return ChecknoteRequestLogLevel.Information;
    }

    public static string GetArea(PathString path)
    {
        if (path.StartsWithSegments("/api"))
        {
            return "api";
        }

        if (path == "/health")
        {
            return "health";
        }

        if (path == HelloWorldRoute || path == $"{HelloWorldRoute}/")
        {
            return "api";
        }

        string? value = path.Value;

        if (!string.IsNullOrWhiteSpace(value) && Path.HasExtension(value))
        {
            return "static";
        }

        return "web";
    }
}

public enum ChecknoteRequestLogLevel
{
    Information,
    Warning,
    Error,
}
