using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Receipts;

public class UpdateReceipt : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPut("/", Handle)
        .WithRequestValidation<Request>()
        .WithEnsureEntityExists<Receipt, Request>()
        .WithUniqueProperty<Request, Receipt, string>(entity => entity.Number, request => request.Number)
        .WithEntityArchiveValidation<Request, Unit>(
            r => r.IncomeResources.Select(incomeResource => incomeResource.UnitId),
            builder => builder.WithArchiveCheck("Used units archived")
                .ForEditScenario<IncomeResource>(req =>
                    incomeResources => incomeResources
                        .Where(ir => ir.ReceiptId == req.Id)
                        .Select(ir => ir.UnitId)))
        .WithEntityArchiveValidation<Request, Resource>(
            r => r.IncomeResources.Select(incomeResource => incomeResource.ResourceId),
            builder => builder.WithArchiveCheck("Used resources archived")
                .ForEditScenario<IncomeResource>(req =>
                    incomeResources => incomeResources
                        .Where(ir => ir.ReceiptId == req.Id)
                        .Select(ir => ir.ResourceId)));

    public record Request(int Id, string Number, DateTimeOffset Date, List<IncomeResourceRequest> IncomeResources)
        : EntityRequest(Id);

    public record IncomeResourceRequest(int? Id, int ResourceId, int UnitId, decimal Quantity);

    public record Response(int Id);

    public class RequestValidator : BaseEntityRequestValidator<Request>
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
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }

    private static async Task<Ok<Response>> Handle(Request request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var receipt = await database.Receipts
            .Include(r => r.IncomeResources)
            .SingleAsync(r => r.Id == request.Id, cancellationToken);

        receipt.Number = request.Number;
        receipt.Date = request.Date;

        var incomeResourceIdsFromRequest = request.IncomeResources
            .Where(ir => ir.Id.HasValue)
            .Select(ir => ir.Id!.Value)
            .ToHashSet();

        receipt.IncomeResources.RemoveAll(r => !incomeResourceIdsFromRequest.Contains(r.Id));

        foreach (var resourceRequest in request.IncomeResources)
        {
            var existing = resourceRequest.Id.HasValue
                ? receipt.IncomeResources.SingleOrDefault(r => r.Id == resourceRequest.Id.Value)
                : null;

            if (existing != null)
            {
                existing.ResourceId = resourceRequest.ResourceId;
                existing.UnitId = resourceRequest.UnitId;
                existing.Quantity = resourceRequest.Quantity;
            }
            else
            {
                receipt.IncomeResources.Add(new IncomeResource
                {
                    ResourceId = resourceRequest.ResourceId,
                    UnitId = resourceRequest.UnitId,
                    Quantity = resourceRequest.Quantity
                });
            }
        }

        await database.SaveChangesAsync(cancellationToken);


        return TypedResults.Ok(new Response(receipt.Id));
    }
}