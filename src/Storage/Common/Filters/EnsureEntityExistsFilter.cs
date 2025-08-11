using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Data.Entities;
using Storage.Common.Results;

namespace Storage.Common.Filters;

public class EnsureEntityExistsFilter<TRequest, TEntity>(StorageDbContext database, Func<TRequest, int?> idSelector)
    : IEndpointFilter where TEntity : class, IEntity
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().Single();
        var cancellationToken = context.HttpContext.RequestAborted;
        var id = idSelector(request);

        if (!id.HasValue)
            return ProblemFactory.BadRequest(context.HttpContext, "Id is required.");

        var exists = await database.Set<TEntity>().AnyAsync(x => x.Id == id.Value, cancellationToken);
        
        return exists
            ? await next(context)
            : ProblemFactory.NotFound(context.HttpContext,
                $"{typeof(TEntity).Name} with id {id} was not found.");
    }
}