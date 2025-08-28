using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EstateAccessManagement.API.Filters
{
    public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, "An unhandled exception occurred.");

            var (statusCode, title, detail) = context.Exception switch
            {
                ValidationException => (400, "Validation Error", context.Exception.Message),
                UnauthorizedAccessException => (401, "Unauthorized", "Authentication required"),
                ArgumentException => (400, "Bad Request", context.Exception.Message),
                KeyNotFoundException => (404, "Not Found", "Resource not found"),
                InvalidOperationException => (409, "Conflict", context.Exception.Message),
                _ => (500, "An unexpected error occurred.", "The server encountered an error. Please try again later.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
