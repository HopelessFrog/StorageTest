using FluentValidation;
using Storage.Common.Results;

namespace Storage.Common.Filters;

public class RequestValidationFilter<TRequest>(IValidator<TRequest>? validator = null) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (validator is null) return await next(context);

        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null) return ProblemFactory.BadRequest(context.HttpContext, "Request  is required.");

        var validationResult = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!validationResult.IsValid) return TypedResults.ValidationProblem(validationResult.ToDictionary());

        return await next(context);
    }
}