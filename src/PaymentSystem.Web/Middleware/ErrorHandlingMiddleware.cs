using Microsoft.AspNetCore.Mvc;
using NLog;

namespace PaymentSystem.Web.Middleware
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            var traceId = httpContext.TraceIdentifier;
            _logger.Error(ex, $"TraceId={traceId} | {ex.Message}");

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "Something went wrong.",
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}