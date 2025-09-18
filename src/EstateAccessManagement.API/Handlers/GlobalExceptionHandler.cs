using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EstateAccessManagement.API.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var (statusCode, title, detail) = exception switch
            {
                ValidationException => (400, "Validation Error", exception.Message),
                UnauthorizedAccessException => (401, "Unauthorized", "Authentication required"),
                ArgumentException => (400, "Bad Request", exception.Message),
                KeyNotFoundException => (404, "Not Found", "Resource not found"),
                InvalidOperationException => (409, "Conflict", exception.Message),
                _ => (500, "An unexpected error occurred.", "The server encountered an unexpected error.")
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true;
        }
    }
}
