using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
namespace OrderGrid.Api.Tests;
public class OrderGridApiFactory : WebApplicationFactory<Program>
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"ordergrid-{Guid.NewGuid():N}.db");
    protected virtual string AuthenticationMode => "Demo";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Authentication:Mode", AuthenticationMode);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = AuthenticationMode,
                ["Infrastructure:DatabaseProvider"] = "Sqlite",
                ["Infrastructure:DatabaseConnectionString"] = $"Data Source={_path}",
                ["Infrastructure:InitializeDatabase"] = "true",
                ["Infrastructure:SeedDemoData"] = "true",
                ["Infrastructure:MessagingMode"] = "Local"
            }));
    }
    protected override void Dispose(bool disposing)
    { base.Dispose(disposing); if (disposing && File.Exists(_path)) File.Delete(_path); }
}
internal sealed class EntraOrderGridApiFactory : OrderGridApiFactory
{ protected override string AuthenticationMode => "EntraId"; }
