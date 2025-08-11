using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Receipts;

public class DeleteReceipt : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{id}", Handle)
        .WithRequestValidation<EntityRequest>()
        .WithEnsureEntityExists<Receipt, EntityRequest>();

    private static async Task<Results<NoContent, NotFound>> Handle(
        [AsParameters] EntityRequest request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var rowsDeleted = await database.Receipts
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsDeleted == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}