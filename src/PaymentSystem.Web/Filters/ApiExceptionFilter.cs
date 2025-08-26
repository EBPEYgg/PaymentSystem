using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using PaymentSystem.Domain.Exceptions;
using PaymentSystem.Web.Models;
using System.Net;

namespace PaymentSystem.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public void OnException(ExceptionContext context)
    {
        var traceId = context.HttpContext.TraceIdentifier;
        var exception = context.Exception;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case EntityNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;

            case DuplicateEntityException:
                statusCode = HttpStatusCode.Conflict;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                break;
        }

        _logger.Error(exception, $"TraceId={traceId} | {exception.Message}");

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = exception.Message,
            Description = "",
            TraceId = traceId
        };

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
    }
}