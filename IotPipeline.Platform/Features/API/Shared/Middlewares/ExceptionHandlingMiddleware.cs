using FluentValidation;
using IotPipeline.Platform.Common.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IotPipeline.Platform.Features.API.Shared.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var targetException = GetTargetException(exception);

        int statusCode = GetStatusCode(targetException);
        List<string> errors = GetErrors(targetException);

        LogException(statusCode, targetException);
        var result = Result<NoContent>.Failure(errors);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }

    private static Exception GetTargetException(Exception exception)
    {
        if (exception is MassTransit.RequestException && exception.InnerException != null)
        {
            return exception.InnerException;
        }

        return exception;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static List<string> GetErrors(Exception exception) => exception switch
    {
        ValidationException validationException => [.. validationException.Errors.Select(e => e.ErrorMessage)],
        _ => [exception.Message]
    };

    private void LogException(int statusCode, Exception exception)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        }
        else if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(exception, "Exception occurred: {Message}", exception.Message);
        }
    }
}