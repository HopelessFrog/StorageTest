using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;

namespace Storage.Endpoints.Units;

public class GetUnit : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{id}", Handle)
        .WithRequestValidation<EntityRequest>();

    public record Response(int Id, string Name);

    private static async Task<Results<Ok<Response>, NotFound>> Handle([AsParameters] EntityRequest request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var unit = await database.Units
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(u => new Response(u.Id, u.Name))
            .SingleOrDefaultAsync(cancellationToken);

        return unit is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(unit);
    }
}