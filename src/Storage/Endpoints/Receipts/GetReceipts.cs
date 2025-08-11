using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;

namespace Storage.Endpoints.Receipts;

public class GetReceipts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithRequestValidation<Request>();

    public record Request(
        DateTimeOffset From,
        DateTimeOffset To,
        string[]? Numbers,
        int[]? ResourceIds,
        int[]? UnitIds,
        int? Page,
        int? PageSize) : IPagedRequest;

    public record IncomeResourceResponse(int Id, string Resource, string Unit, decimal Quantity);
    public record Response(int Id, string Number, DateTimeOffset Date, List<IncomeResourceResponse> IncomeResources);

    public class RequestValidator : PagedRequestValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.From)
                .NotNull()
                .Must(date => date.Offset == TimeSpan.Zero);

            RuleFor(x => x.To)
                .NotNull()
                .Must(date => date.Offset == TimeSpan.Zero);

            RuleFor(x => x)
                .Must(r => r.From <= r.To);
        }
    }

    private static async Task<PagedList<Response>> Handle([AsParameters] Request request, StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var query = database.Receipts
            .AsNoTracking()
            .Where(d => d.Date >= request.From && d.Date <= request.To)
            .AsQueryable();

        if (request.Numbers is { Length: > 0 })
            query = query.Where(x => request.Numbers!.Contains(x.Number));

        if (request.ResourceIds is { Length: > 0 })
            query = query.Where(x => x.IncomeResources.Any(r => request.ResourceIds!.Contains(r.ResourceId)));

        if (request.UnitIds is { Length: > 0 })
            query = query.Where(x => x.IncomeResources.Any(r => request.UnitIds!.Contains(r.UnitId)));

        return await query
            .OrderByDescending(d => d.Date).ThenBy(d => d.Id)
            .Select(d => new Response(
                d.Id,
                d.Number,
                d.Date,
                d.IncomeResources
                    .OrderByDescending(r => r.Quantity)
                    .ThenBy(r => r.Id)
                    .Select(x => new IncomeResourceResponse(
                        x.Id,
                        x.Resource.Name,
                        x.Unit.Name,
                        x.Quantity))
                    .ToList()))
            .ToPagedListAsync(request, cancellationToken);
    }
}