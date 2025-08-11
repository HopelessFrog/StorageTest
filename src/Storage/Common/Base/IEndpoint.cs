namespace Storage.Common.Base;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}