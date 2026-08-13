using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace OrderGrid.Api.Security;
public sealed class DemoAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Demo";
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tenant = Request.Headers["X-Tenant-ID"].FirstOrDefault() ?? "demo";
        var actor = Request.Headers["X-Demo-User"].FirstOrDefault() ?? "portfolio.operator";
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, actor), new(ClaimTypes.Name, actor),
            new(ClaimTypes.Role, "operator"), new("tenant_id", tenant),
            new("scp", "orders.read orders.write operations.read")
        ];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
