using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Resources;

public class ArchiveResource : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPatch("/{id}/archive", Handle)
        .WithRequestValidation<EntityRequest>()
        .WithEnsureEntityExists<Resource, EntityRequest>();

    private static async Task<NoContent> Handle([AsParameters] EntityRequest request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        await database.Resources
            .Where(x => x.Id == request.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.State, ArchiveState.Archived),
                cancellationToken);

        return TypedResults.NoContent();
    }
}