using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Resources;

public class UpdateResource : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPatch("/", Handle)
        .WithRequestValidation<Request>()
        .WithEnsureEntityExists<Resource, Request>()
        .WithUniqueProperty<Request, Resource, string>(entity => entity.Name, request => request.Name);

    public record Request(int Id, string Name) : EntityRequest(Id);

    public class RequestValidator : BaseEntityRequestValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);
        }
    }

    private static async Task<NoContent> Handle(Request request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        await database.Resources.Where(r => r.Id == request.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(u => u.Name, request.Name), cancellationToken);

        return TypedResults.NoContent();
    }
}