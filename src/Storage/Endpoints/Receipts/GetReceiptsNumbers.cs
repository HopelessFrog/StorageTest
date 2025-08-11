using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Data;

namespace Storage.Endpoints.Receipts;

public class GetReceiptsNumbers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/numbers", Handle);

    public record Response(string Number);

    private static async Task<List<Response>> Handle(StorageDbContext database, CancellationToken cancellationToken)
    {
        return await database.Receipts
            .AsNoTracking()
            .Select(x => x.Number)
            .Distinct()
            .OrderBy(x => x)
            .Select(n => new Response(n))
            .ToListAsync(cancellationToken);
    }
}