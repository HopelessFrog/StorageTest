using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Units;

public class DeleteUnit : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{id}", Handle)
        .WithRequestValidation<EntityRequest>()
        .WithEnsureEntityExists<Unit, EntityRequest>()
        .WithDeletionDependencyCheck<IncomeResource, Unit, EntityRequest>(id =>
            incomeResource => incomeResource.UnitId == id);
    
    private static async Task<Results<NoContent, NotFound>> Handle(
        [AsParameters] EntityRequest request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var rowsDeleted = await database.Units
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsDeleted == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}