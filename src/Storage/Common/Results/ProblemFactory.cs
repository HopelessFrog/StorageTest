using Microsoft.AspNetCore.Http.HttpResults;

namespace Storage.Common.Results;

public static class ProblemFactory
{
    private static ProblemHttpResult Create(HttpContext httpContext,
        int statusCode,
        string title,
        string? detail = null,
        string type = "about:blank",
        Dictionary<string, object?>? extraExtensions = null)
    {
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = httpContext.TraceIdentifier,
            ["instance"] = httpContext.Request.Path.ToString()
        };

        if (extraExtensions is not null)
        {
            foreach (var kv in extraExtensions)
            {
                extensions[kv.Key] = kv.Value;
            }
        }

        return TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            type: type,
            extensions: extensions);
    }

    public static ProblemHttpResult BadRequest(HttpContext httpContext, string detail) =>
        Create(httpContext, StatusCodes.Status400BadRequest, "Bad Request", detail, type: "urn:problem:bad-request");

    public static ProblemHttpResult NotFound(HttpContext httpContext, string detail) =>
        Create(httpContext, StatusCodes.Status404NotFound, "Not Found", detail, type: "urn:problem:not-found");

    public static ProblemHttpResult Conflict(HttpContext httpContext, string title, string detail, string type) =>
        Create(httpContext, StatusCodes.Status409Conflict, title, detail, type: type);

    public static ProblemHttpResult InternalServerError(HttpContext httpContext, string? detail = null) =>
        Create(httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error",
            detail ?? "An unexpected error occurred.", type: "urn:problem:internal-error");

    public static ProblemHttpResult UniqueConstraintViolation(HttpContext httpContext) =>
        Conflict(httpContext, "Unique constraint violation",
            "A record with the same value already exists.",
            type: "urn:problem:unique-violation");

    public static ProblemHttpResult ForeignKeyViolation(HttpContext httpContext) =>
        Conflict(httpContext, "Foreign key constraint violation",
            "Cannot delete entity because it is referenced by other records.",
            type: "urn:problem:foreign-key-violation");
}


