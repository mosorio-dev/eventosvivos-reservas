using System.Text.Json;
using EventosVivos.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using ValidationException = EventosVivos.Application.Common.Exceptions.ValidationException;

namespace EventosVivos.Api.Middleware;

/// <summary>
/// Translates exceptions thrown anywhere in the pipeline into RFC 7807 ProblemDetails.
/// Keeps controllers free of try/catch and guarantees a consistent error contract.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title, problem) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                BuildValidationProblem(validation)),

            UnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                "No autorizado",
                new ProblemDetails { Detail = unauthorized.Message }),

            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                new ProblemDetails { Detail = notFound.Message }),

            BusinessRuleViolationException business => (
                StatusCodes.Status409Conflict,
                "Conflicto con una regla de negocio",
                BuildBusinessProblem(business)),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                new ProblemDetails { Detail = "Ocurrió un error inesperado." })
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        problem.Status = status;
        problem.Title = title;
        problem.Type = $"https://httpstatuses.io/{status}";

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }

    private static ProblemDetails BuildValidationProblem(ValidationException exception)
    {
        var problem = new ProblemDetails { Detail = exception.Message };
        problem.Extensions["errors"] = exception.Errors;
        return problem;
    }

    private static ProblemDetails BuildBusinessProblem(BusinessRuleViolationException exception)
    {
        var problem = new ProblemDetails { Detail = exception.Message };
        problem.Extensions["code"] = exception.Code;
        return problem;
    }
}
