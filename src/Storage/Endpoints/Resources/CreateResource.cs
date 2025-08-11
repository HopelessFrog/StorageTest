using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Data;
using Storage.Data.Entities;

namespace Storage.Endpoints.Resources;

public class CreateResource : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/", Handle)
        .WithRequestValidation<Request>()
        .WithUniqueProperty<Request, Resource, string>(entity => entity.Name, request => request.Name);

    public record Request(string Name);

    public record Response(int Id);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);
        }
    }

    private static async Task<Created<Response>> Handle(Request request,
        StorageDbContext database,
        CancellationToken cancellationToken)
    {
        var resource = new Resource
        {
            Name = request.Name
        };

        await database.Resources.AddAsync(resource, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        var response = new Response(resource.Id);

        return TypedResults.Created($"/resources/{resource.Id}", response);
    }
}