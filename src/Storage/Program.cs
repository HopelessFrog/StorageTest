using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Storage.Common.Extensions;
using Storage.Common.Middlewares;
using Storage.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<StorageDbContext>("storage-db");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var httpContext = context.HttpContext;
        context.ProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        context.ProblemDetails.Instance = httpContext.Request.Path;
        context.ProblemDetails.Type ??= "about:blank";
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapEndpoints();

await app.EnsureDatabaseCreated();

app.Run();