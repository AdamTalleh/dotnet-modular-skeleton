using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Skeleton.Host;
using Skeleton.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

// Every module is listed here explicitly. Add a module = add one entry.
IModule[] modules =
[
];

builder.AddObservability();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddWebDefaults(builder.Configuration);

// Auth hooks: each product adds its scheme here (e.g. .AddJwtBearer(...)) and a default/fallback policy.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

foreach (var module in modules)
{
    module.Register(builder.Services, builder.Configuration);
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Liveness runs no checks (process is up); readiness runs every registered check.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

await app.RunAsync();
