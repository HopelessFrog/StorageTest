using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;

namespace Storage.Endpoints.Receipts;

public class GetReceiptsResource : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/resources", Handle);

    public record Response(int Id, string Name);

    private static async Task<List<Response>> Handle(
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        return await database.IncomeResources
            .AsNoTracking()
            .Select(ir => new { ir.Resource.Id, ir.Resource.Name })
            .Distinct()
            .OrderBy(x => x.Id)
            .Select(x => new Response(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}

