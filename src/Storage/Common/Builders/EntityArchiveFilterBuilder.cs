using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Common.Builders;

public class EntityArchiveFilterBuilder<TRequest, TEntity>(
    StorageDbContext database,
    Func<TRequest, IEnumerable<int>> idsFromRequestSelector)
    where TEntity : class, IEntity, IArchivable
{
    private Func<StorageDbContext, TRequest, Task<IEnumerable<int>>>? _alreadySelectedIdsSelectorAsync;
    private string _errorMessage = "One or more entities are archived.";
    private bool _withArchiveCheck;

    public EntityArchiveFilterBuilder<TRequest, TEntity> WithArchiveCheck(string errorMessage)
    {
        _errorMessage = errorMessage;
        _withArchiveCheck = true;
        return this;
    }

    public EntityArchiveFilterBuilder<TRequest, TEntity> ForEditScenario<TOwner>(
        Func<TRequest, Func<IQueryable<TOwner>, IQueryable<int>>> queryFactory)
        where TOwner : class
    {
        _alreadySelectedIdsSelectorAsync = async (db, request) =>
        {
            var dbSet = db.Set<TOwner>();
            var query = queryFactory(request);
            return await query(dbSet).ToListAsync();
        };

        return this;
    }

    public IEndpointFilter Build()
    {
        return new GenericEntityValidationFilter(
            database,
            request => idsFromRequestSelector((TRequest)request),
            _alreadySelectedIdsSelectorAsync,
            _withArchiveCheck,
            _errorMessage
        );
    }

    private class GenericEntityValidationFilter(
        StorageDbContext dbContext,
        Func<object, IEnumerable<int>> idsFromRequestSelector,
        Func<StorageDbContext, TRequest, Task<IEnumerable<int>>>? alreadySelectedIdsSelectorAsync,
        bool withArchiveCheck,
        string errorMessage)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var request = context.Arguments.OfType<TRequest>().Single();

            var idsFromRequest = idsFromRequestSelector(request).Distinct().ToList();

            var alreadySelectedIds = new HashSet<int>();

            if (alreadySelectedIdsSelectorAsync != null)
            {
                var selectedIds = await alreadySelectedIdsSelectorAsync(dbContext, request);
                alreadySelectedIds = selectedIds.ToHashSet();
            }

            var idsToCheck = idsFromRequest.Except(alreadySelectedIds).ToList();

            if (!idsToCheck.Any()) return await next(context);

            var query = dbContext.Set<TEntity>().Where(e => idsToCheck.Contains(e.Id));

            if (withArchiveCheck) query = query.Where(entity => entity.State == ArchiveState.Active);

            var validIdsFromDb = await query.Select(e => e.Id).ToListAsync(context.HttpContext.RequestAborted);
            var invalidIds = idsToCheck.Except(validIdsFromDb).ToList();

            if (invalidIds.Any())
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["ids"] = new[] { errorMessage, $"Invalid IDs: {string.Join(", ", invalidIds)}" }
                });
            }

            return await next(context);
        }
    }
}