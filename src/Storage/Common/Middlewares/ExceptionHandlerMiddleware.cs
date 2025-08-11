using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Storage.Common.Results;

namespace Storage.Common.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation
                                           })
        {
            var result = ProblemFactory.UniqueConstraintViolation(context);
            await result.ExecuteAsync(context);
        }
        catch (PostgresException ex) when (ex is { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            var result = ProblemFactory.ForeignKeyViolation(context);
            await result.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            await ProblemFactory.InternalServerError(context).ExecuteAsync(context);
        }
    }
}