namespace Checknote.Api.UnitTests.App.Middleware;

using Checknote.Api.Middleware;
using Checknote.Api.UnitTests.Support;
using Microsoft.AspNetCore.Http;

internal static class ErrorResponsePolicyTests
{
    public static void Run()
    {
        TestAssert.True(
            ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/", "GET", "text/html,application/xhtml+xml")),
            "root browser document request should receive the friendly error page");
        TestAssert.True(
            ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/client/route", "GET", "text/html")),
            "SPA browser document request should receive the friendly error page");
        TestAssert.True(
            !ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/api/todos", "GET", "text/html")),
            "API requests should not receive the friendly error page");
        TestAssert.True(
            !ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/client/route", "GET", "application/json")),
            "JSON consumers should not receive the friendly error page");
        TestAssert.True(
            !ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/main-ABC123.js", "GET", "text/html")),
            "static asset requests should not receive the friendly error page");
        TestAssert.True(
            !ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/error-assets/friendly-error-test-pattern.png", "GET", "image/png")),
            "friendly error asset requests should not receive the friendly error page");
        TestAssert.True(
            !ErrorResponsePolicy.ShouldServeFriendlyErrorPage(
                CreateRequest("/", "OPTIONS", "text/html")),
            "preflight requests should not receive the friendly error page");
    }

    private static HttpRequest CreateRequest(
        string path,
        string method,
        string accept)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Request.Headers.Accept = accept;

        return context.Request;
    }
}
