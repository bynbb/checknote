namespace Checknote.Api.Middleware;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string NoCacheHeader = "no-cache, no-store, must-revalidate";
    private readonly ILogger<GlobalExceptionHandler> logger;
    private readonly IWebHostEnvironment environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment)
    {
        this.logger = logger;
        this.environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred.");

        if (ErrorResponsePolicy.ShouldServeFriendlyErrorPage(httpContext.Request) &&
            TryGetFriendlyErrorPagePath(out string errorPagePath))
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            httpContext.Response.Headers.CacheControl = NoCacheHeader;
            httpContext.Response.Headers.Pragma = "no-cache";
            httpContext.Response.Headers.Expires = "0";

            await httpContext.Response.SendFileAsync(
                errorPagePath,
                0,
                null,
                cancellationToken);

            return true;
        }

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Server failure",
            Detail = "An unexpected error occurred.",
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private bool TryGetFriendlyErrorPagePath(out string errorPagePath)
    {
        errorPagePath = ErrorResponsePolicy.GetErrorPagePath(environment);

        return File.Exists(errorPagePath);
    }
}
