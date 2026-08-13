using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderGrid.Api.Configuration;
using OrderGrid.Api.Endpoints;
using OrderGrid.Api.Middleware;
using OrderGrid.Api.Security;
using OrderGrid.Application;
using OrderGrid.Infrastructure;
using OrderGrid.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var authMode = builder.Configuration["Authentication:Mode"] ?? "Demo";
var scheme = authMode.Equals("EntraId", StringComparison.OrdinalIgnoreCase)
    ? JwtBearerDefaults.AuthenticationScheme : DemoAuthenticationHandler.SchemeName;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = scheme;
    options.DefaultChallengeScheme = scheme;
})
.AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(DemoAuthenticationHandler.SchemeName, _ => { })
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.Audience = builder.Configuration["Authentication:Audience"];
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name", RoleClaimType = "roles", ValidateIssuer = true,
        ValidateAudience = true, ValidateLifetime = true
    };
});
builder.Services.AddAuthorization(AuthorizationPolicies.Configure);
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    builder.Services.AddOpenTelemetry().UseAzureMonitor();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new
{ service = "OrderGrid API", version = "1.0.0", documentation = "/openapi/v1.json", health = "/health/ready" })).AllowAnonymous();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" })).AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapOpenApi();
app.MapOrderEndpoints();
app.MapOperationsEndpoints();

var infrastructure = app.Services.GetRequiredService<InfrastructureOptions>();
if (infrastructure.InitializeDatabase)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>()
        .InitializeAsync(infrastructure.SeedDemoData, CancellationToken.None);
}
await app.RunAsync();
public partial class Program;
