using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SolutionPruebaTecnica.Presentation.Exceptions;

public class ExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var (statusCode, errorResponse) = GetErrorResponse(context.Exception);
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = 500,
            ContentTypes = { "application/json" }
        };
        _logger.LogError(context.Exception, "Returning error response: {ErrorResponse}", context.Exception.Message);
    }

    private static (int statusCode, ErrorResponse errorResponse) GetErrorResponse(Exception ex)
    {
        if (ex is ArgumentNullException || ex is ArgumentException)
        {
            return (400, ErrorResponse.FromException(ex, 400));
        }
        if (ex is KeyNotFoundException)
        {
            return (404, ErrorResponse.FromException(ex, 404));
        }
        if (ex is UnauthorizedAccessException)
        {
            return (401, ErrorResponse.FromException(ex, 401));
        }
        if (ex is TimeoutException)
        {
            return (504, ErrorResponse.FromException(ex, 504));
        }
        if (ex is HttpRequestException)
        {
            return (502, ErrorResponse.FromException(ex, 502));
        }
        if (ex is InvalidOperationException)
        {
            return (409, ErrorResponse.FromException(ex, 409));
        }
        return (500, ErrorResponse.FromException(ex, 500));
    }

}