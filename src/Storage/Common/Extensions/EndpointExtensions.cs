using Storage.Common.Base;
using Storage.Endpoints.Receipts;
using Storage.Endpoints.Resources;
using Storage.Endpoints.Units;

namespace Storage.Common.Extensions;

public static class EndpointExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("/api").WithOpenApi();

        endpoints
            .MapUnits()
            .MapResources()
            .MapReceipts();
    }

    private static IEndpointRouteBuilder MapUnits(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/units")
            .WithTags("Units");


        endpoints
            .MapEndpoint<GetUnits>()
            .MapEndpoint<GetUnit>()
            .MapEndpoint<CreateUnit>()
            .MapEndpoint<UpdateUnit>()
            .MapEndpoint<DeleteUnit>()
            .MapEndpoint<ArchiveUnit>()
            .MapEndpoint<UnarchiveUnit>();

        return app;
    }

    private static IEndpointRouteBuilder MapResources(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/resources")
            .WithTags("Resources");

        endpoints
            .MapEndpoint<GetResources>()
            .MapEndpoint<GetResource>()
            .MapEndpoint<CreateResource>()
            .MapEndpoint<UpdateResource>()
            .MapEndpoint<DeleteResource>()
            .MapEndpoint<ArchiveResource>()
            .MapEndpoint<UnarchiveResource>();


        return app;
    }

    private static IEndpointRouteBuilder MapReceipts(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/receipts")
            .WithTags("Receipts");

        endpoints
            .MapEndpoint<GetReceipts>()
            .MapEndpoint<GetReceiptsNumbers>()
            .MapEndpoint<GetReceiptsUnits>()
            .MapEndpoint<GetReceiptsResource>()
            .MapEndpoint<GetIncomeResourcesByReceiptId>()
            .MapEndpoint<CreateReceipt>()
            .MapEndpoint<UpdateReceipt>()
            .MapEndpoint<DeleteReceipt>();

        return app;
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}