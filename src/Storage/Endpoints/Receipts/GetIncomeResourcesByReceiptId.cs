using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Data;

namespace Storage.Endpoints.Receipts;

public class GetIncomeResourcesByReceiptId : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{id}/income-resources", Handle)
        .WithRequestValidation<Request>();

    public record Request(int Id);

    public record Response(int Id, int ResourceId, string Resource, int UnitId, string Unit, decimal Quantity);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    private static async Task<List<Response>> Handle([AsParameters] Request request, StorageDbContext database,
        CancellationToken cancellationToken)
    {
        return await database.IncomeResources
            .AsNoTracking()
            .Where(x => x.ReceiptId == request.Id)
            .OrderByDescending(r => r.Quantity)
            .ThenBy(r => r.Id)
            .Select(x => new Response(x.Id, x.ResourceId, x.Resource.Name, x.UnitId, x.Unit.Name, x.Quantity))
            .ToListAsync(cancellationToken);
    }
}