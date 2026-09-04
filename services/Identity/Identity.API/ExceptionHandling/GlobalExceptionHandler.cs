using Identity.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Identity.API.ExceptionHandling;

internal sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode =
            exception switch
            {
                UnauthorizedException =>
                    StatusCodes.Status401Unauthorized,

                ConflictException =>
 StatusCodes.Status409Conflict,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");
        }

        httpContext.Response.StatusCode =
            statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                status = statusCode,
                error = GetMessage(
                    exception,
                    statusCode)
            },
            cancellationToken);

        return true;
    }

    private static string GetMessage(
        Exception exception,
        int statusCode)
    {
        if (statusCode ==
            StatusCodes.Status500InternalServerError)
        {
            return "An unexpected error occurred.";
        }

        return exception.Message;
    }
}
