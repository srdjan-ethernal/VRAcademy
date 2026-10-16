using System.Text.Json;
using Asp.Versioning;
using Carter;
using FluentValidation;
using Kodster.CQRS;
using Microsoft.AspNetCore.Http.Json;
using VRAcademy.Simulations.Api.Authentication.Extensions;
using VRAcademy.Simulations.Api.Reports.Options;
using VRAcademy.Simulations.Api.Reports.Repositories;
using VRAcademy.Simulations.Api.Validation.Extensions;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddApiKeyAuthentication();

builder.Services.AddCarter();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "nswd:";
});

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddRequestHandlersFromAssembly(assembly);

builder.Services.AddOptionsWithFluentValidation<ReportCacheOptions>(ReportCacheOptions.SectionName);

builder.Services.AddScoped<IReportRepository, ReportRepository>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var versionedGroup = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

versionedGroup.MapCarter();

app.Run();
