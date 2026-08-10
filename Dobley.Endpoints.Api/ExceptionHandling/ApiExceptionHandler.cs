using System.Text.Json;
using Dobley.Domain.Core.Errors.Entities;
using Microsoft.AspNetCore.Diagnostics;

namespace Dobley.Endpoints.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private const string DOMAIN_VALIDATION_LOG_MESSAGE = "Доменная валидация не пройдена для {Method} {Path}";
    private const string BAD_REQUEST_LOG_MESSAGE = "Некорректное тело запроса для {Method} {Path}";
    private const string BAD_REQUEST_RESPONSE_MESSAGE = "Некорректное тело запроса.";
    private const string UNHANDLED_EXCEPTION_LOG_MESSAGE = "Необработанная ошибка для {Method} {Path}";
    private const string UNHANDLED_EXCEPTION_RESPONSE_MESSAGE = "Внутренняя ошибка сервера.";

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainValidateException)
        {
            logger.LogWarning(exception, DOMAIN_VALIDATION_LOG_MESSAGE, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message },
                cancellationToken: cancellationToken);

            return true;
        }

        if (exception is BadHttpRequestException or JsonException)
        {
            logger.LogWarning(exception, BAD_REQUEST_LOG_MESSAGE, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = BAD_REQUEST_RESPONSE_MESSAGE },
                cancellationToken: cancellationToken);

            return true;
        }

        logger.LogError(exception, UNHANDLED_EXCEPTION_LOG_MESSAGE, context.Request.Method, context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = UNHANDLED_EXCEPTION_RESPONSE_MESSAGE },
            cancellationToken: cancellationToken);

        return true;
    }
}
