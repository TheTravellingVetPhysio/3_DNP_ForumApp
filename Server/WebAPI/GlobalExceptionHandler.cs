
namespace WebAPI;

using Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            BusinessConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                400 => "Bad Request",
                404 => "Not Found",
                409 => "Conflict",
                _ => "Internal Server Error"
            },
            Detail = statusCode == 500
                ? "Der opstod en uventet fejl."
                : exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
