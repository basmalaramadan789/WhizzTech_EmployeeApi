using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Domain.Exceptions;

namespace WhizzTech.EmployeeApi.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, errorMessage) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            EmployeeNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            DuplicateEmailException => (StatusCodes.Status409Conflict, exception.Message),
            InvalidTenantException => (StatusCodes.Status400BadRequest, exception.Message),
            CustomDataValidationException => (StatusCodes.Status422UnprocessableEntity, exception.Message),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Failure(errorMessage);
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
