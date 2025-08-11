using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Resources;

public class DeleteResource : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{id}", Handle)
        .WithRequestValidation<EntityRequest>()
        .WithEnsureEntityExists<Resource, EntityRequest>(u => u.Id)
        .WithDeletionDependencyCheck<IncomeResource, Resource, EntityRequest>(id =>
            resource => resource.ResourceId == id);

    private static async Task<Results<NoContent, NotFound>> Handle([AsParameters] EntityRequest request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var rowsDeleted = await database.Resources
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsDeleted == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}