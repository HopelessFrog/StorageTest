using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Storage.Common;
using Storage.Common.Base;
using Storage.Common.Extensions;
using Storage.Common.Requests;
using Storage.Data;

namespace Storage.Endpoints.Units;

public class GetUnits : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithRequestValidation<Request>();


    public record Request(int? Page, int? PageSize, ArchiveState State = ArchiveState.Active) : IPagedRequest;

    public record Response(int Id, string Name);

    public class RequestValidator : PagedRequestValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.State).IsInEnum();
        }
    }

    private static async Task<PagedList<Response>> Handle([AsParameters] Request request, StorageDbContext database,
        CancellationToken cancellationToken)
    {
        return await database.Units
            .AsNoTracking()
            .Where(u => u.State == request.State)
            .OrderBy(u => u.Name).ThenBy(u => u.Id)
            .Select(u => new Response(u.Id, u.Name))
            .ToPagedListAsync(request, cancellationToken);
    }
}