using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Data.Entities;
using Storage.Common.Results;

namespace Storage.Common.Filters;

public class DeletionDependencyFilter<TRequest, TEntity, TDependentEntity>(
    StorageDbContext database,
    Func<TRequest, int?> idSelector,
    Func<int, Expression<Func<TDependentEntity, bool>>> dependencyPredicateFactory)
    : IEndpointFilter where TDependentEntity : class, IEntity
    where TEntity : class, IEntity
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().Single();
        var id = idSelector(request);

        if (!id.HasValue) return await next(context);

        var predicate = dependencyPredicateFactory(id.Value);
        var cancellationToken = context.HttpContext.RequestAborted;
        var hasDependenciesResult = await database.Set<TDependentEntity>().AnyAsync(predicate, cancellationToken);

        if (hasDependenciesResult)
            return ProblemFactory.Conflict(
                context.HttpContext,
                title: "Deletion conflict",
                detail:
                    $"Cannot delete {typeof(TEntity).Name.ToLowerInvariant()} with dependencies to {typeof(TDependentEntity).Name.ToLowerInvariant()}",
                type: "urn:problem:deletion-conflict");

        return await next(context);
    }
}