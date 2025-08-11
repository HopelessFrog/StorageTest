using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;

namespace Storage.Endpoints.Receipts;

public class GetReceiptsUnits : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/units", Handle);

    public record Response(int Id, string Name);

    private static async Task<List<Response>> Handle(
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        return await database.IncomeResources
            .AsNoTracking()
            .Select(ir => new { ir.Unit.Id, ir.Unit.Name })
            .Distinct()
            .OrderBy(x => x.Id)
            .Select(x => new Response(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}