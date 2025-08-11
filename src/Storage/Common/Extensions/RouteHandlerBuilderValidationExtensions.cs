using System.Linq.Expressions;
using FluentValidation;
using Storage.Common.Builders;
using Storage.Common.Filters;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Common.Extensions;

public static class RouteHandlerBuilderValidationExtensions
{
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
                var filter = new RequestValidationFilter<TRequest>(validator);
                return await filter.InvokeAsync(context, next);
            })
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static RouteHandlerBuilder WithEnsureEntityExists<TEntity, TRequest>(this RouteHandlerBuilder builder,
        Func<TRequest, int?> idSelector) where TEntity : class, IEntity
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();
                var filter = new EnsureEntityExistsFilter<TRequest, TEntity>(db, idSelector);
                return await filter.InvokeAsync(context, next);
            })
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static RouteHandlerBuilder WithEnsureEntityExists<TEntity, TRequest>(this RouteHandlerBuilder builder)
        where TEntity : class, IEntity where TRequest : EntityRequest
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();
                var filter = new EnsureEntityExistsFilter<TRequest, TEntity>(db, entity => entity.Id);
                return await filter.InvokeAsync(context, next);
            })
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static RouteHandlerBuilder WithDeletionDependencyCheck<TDependentEntity, TEntity, TRequest>(
        this RouteHandlerBuilder builder,
        Func<TRequest, int?> idSelector, Func<int, Expression<Func<TDependentEntity, bool>>> dependencyPredicateFactory)
        where TEntity : class, IEntity
        where TDependentEntity : class, IEntity
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var database = context.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();
                var filter =
                    new DeletionDependencyFilter<TRequest, TEntity, TDependentEntity>(database, idSelector,
                        dependencyPredicateFactory);
                return await filter.InvokeAsync(context, next);
            })
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static RouteHandlerBuilder WithDeletionDependencyCheck<TDependentEntity, TEntity, TRequest>(
        this RouteHandlerBuilder builder,
        Func<int, Expression<Func<TDependentEntity, bool>>> dependencyPredicateFactory)
        where TEntity : class, IEntity
        where TDependentEntity : class, IEntity
        where TRequest : EntityRequest
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var database = context.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();
                var filter =
                    new DeletionDependencyFilter<TRequest, TEntity, TDependentEntity>(database, entity => entity.Id,
                        dependencyPredicateFactory);
                return await filter.InvokeAsync(context, next);
            })
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static RouteHandlerBuilder WithUniqueProperty<TRequest, TEntity, TProperty>(
        this RouteHandlerBuilder builder,
        Expression<Func<TEntity, TProperty>> entityProperty,
        Func<TRequest, TProperty> requestProperty)
        where TRequest : class
        where TEntity : class, IEntity
    {
        return builder
            .AddEndpointFilterFactory((endpointFilterFactoryContext, next) => async context =>
            {
                var database = context.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();
                var filter =
                    new UniquePropertyFilter<TRequest, TEntity, TProperty>(database, entityProperty, requestProperty);

                return await filter.InvokeAsync(context, next);
            })
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static RouteHandlerBuilder WithEntityArchiveValidation<TRequest, TEntity>(
        this RouteHandlerBuilder builder,
        Func<TRequest, IEnumerable<int>> idsFromRequestSelector,
        Action<EntityArchiveFilterBuilder<TRequest, TEntity>>? configure = null)
        where TRequest : class
        where TEntity : class, IEntity, IArchivable
    {
        return builder.AddEndpointFilterFactory((filterFactoryContext, next) =>
            {
                return async invocationContext =>
                {
                    var database = invocationContext.HttpContext.RequestServices.GetRequiredService<StorageDbContext>();

                    var filterBuilder =
                        new EntityArchiveFilterBuilder<TRequest, TEntity>(database, idsFromRequestSelector);

                    configure?.Invoke(filterBuilder);

                    var filter = filterBuilder.Build();

                    return await filter.InvokeAsync(invocationContext, next);
                };
            })
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}