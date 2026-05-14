namespace Checknote.Api.Middleware;

using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

public static class ErrorResponsePolicy
{
    public const string ErrorAssetsRequestPath = "/error-assets";
    public const string ErrorPageFileName = "error.html";
    public const string ErrorPagesDirectoryName = "ErrorPages";
    public const string NoCacheHeader = "no-cache, no-store, must-revalidate";

    public static bool ShouldServeFriendlyErrorPage(HttpRequest request)
    {
        if (!HttpMethods.IsGet(request.Method) &&
            !HttpMethods.IsHead(request.Method))
        {
            return false;
        }

        if (request.Path.StartsWithSegments("/api") ||
            request.Path.StartsWithSegments("/swagger") ||
            request.Path.StartsWithSegments(ErrorAssetsRequestPath) ||
            Path.HasExtension(request.Path))
        {
            return false;
        }

        return RequestAcceptsHtml(request);
    }

    public static string GetErrorPagesPath(IHostEnvironment environment) =>
        Path.Combine(environment.ContentRootPath, ErrorPagesDirectoryName);

    public static string GetErrorPagePath(IHostEnvironment environment) =>
        Path.Combine(GetErrorPagesPath(environment), ErrorPageFileName);

    private static bool RequestAcceptsHtml(HttpRequest request)
    {
        string accept = request.Headers.Accept.ToString();

        return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase) ||
            accept.Contains("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
    }
}
