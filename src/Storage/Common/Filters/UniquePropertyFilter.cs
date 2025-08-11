using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Requests;
using Storage.Common.Results;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Common.Filters;

public class UniquePropertyFilter<TRequest, TEntity, TProperty>(
    StorageDbContext database,
    Expression<Func<TEntity, TProperty>> entityProperty,
    Func<TRequest, TProperty> requestProperty)
    : IEndpointFilter
    where TRequest : class
    where TEntity : class, IEntity
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
            return await next(context);

        var propertyValue = requestProperty(request);
        var parameter = entityProperty.Parameters.Single();

        Expression propertyComparison = propertyValue is null
            ? Expression.Equal(entityProperty.Body, Expression.Constant(null, typeof(TProperty)))
            : Expression.Equal(entityProperty.Body, Expression.Constant(propertyValue, typeof(TProperty)));

        Expression<Func<TEntity, bool>> predicate;
        
        if (request is EntityRequest { Id: > 0 } requestWithId)
        {
            var idProperty = Expression.Property(parameter, nameof(IEntity.Id));
            var idNotEquals = Expression.NotEqual(idProperty, Expression.Constant(requestWithId.Id));
            var combined = Expression.AndAlso(idNotEquals, propertyComparison);
            predicate = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
        }
        else
        {
            predicate = Expression.Lambda<Func<TEntity, bool>>(propertyComparison, parameter);
        }

        bool exists = await database.Set<TEntity>()
            .AnyAsync(predicate, context.HttpContext.RequestAborted);

        if (exists)
            return ProblemFactory.UniqueConstraintViolation(context.HttpContext);

        return await next(context);
    }
}
