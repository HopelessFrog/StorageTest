using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Receipts;

public class CreateReceipt : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/", Handle)
        .WithRequestValidation<Request>()
        .WithUniqueProperty<Request, Receipt, string>(entity => entity.Number, request => request.Number)
        .WithEntityArchiveValidation<Request, Unit>(
            r => r.IncomeResources.Select(incomeResource => incomeResource.UnitId),
            builder => builder.WithArchiveCheck("Used units archived"))
        .WithEntityArchiveValidation<Request, Resource>(
            r => r.IncomeResources.Select(incomeResource => incomeResource.ResourceId),
            builder => builder.WithArchiveCheck("Used resources archived"));

    public record Request(string Number, DateTimeOffset Date, List<IncomeResourceRequest> IncomeResources);

    public record IncomeResourceRequest(int ResourceId, int UnitId, decimal Quantity);

    public record Response(int Id);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Date)
                .NotEmpty()
                .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow);

            RuleForEach(x => x.IncomeResources)
                .SetValidator(new IncomeResourceRequestValidator());
        }
    }

    public class IncomeResourceRequestValidator : AbstractValidator<IncomeResourceRequest>
    {
        public IncomeResourceRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }

    private static async Task<Created<Response>> Handle([FromBody] Request request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var receipt = new Receipt
        {
            Number = request.Number,
            Date = request.Date
        };

        await database.Receipts.AddAsync(receipt, cancellationToken);

        var incomeResources = request.IncomeResources.Select(r => new IncomeResource
        {
            ResourceId = r.ResourceId,
            UnitId = r.UnitId,
            Quantity = r.Quantity,
            Receipt = receipt
        }).ToList();

        await database.IncomeResources.AddRangeAsync(incomeResources, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        
        return TypedResults.Created($"/receipts/{receipt.Id}", new Response(receipt.Id));
    }
}