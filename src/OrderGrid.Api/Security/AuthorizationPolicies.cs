using Microsoft.AspNetCore.Authorization;
namespace OrderGrid.Api.Security;
public static class AuthorizationPolicies
{
    public const string ReadOrders = "orders.read";
    public const string WriteOrders = "orders.write";
    public const string ReadOperations = "operations.read";
    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(ReadOrders, p => p.RequireAuthenticatedUser().RequireAssertion(c => Has(c, ReadOrders)));
        options.AddPolicy(WriteOrders, p => p.RequireAuthenticatedUser().RequireAssertion(c => Has(c, WriteOrders)));
        options.AddPolicy(ReadOperations, p => p.RequireAuthenticatedUser().RequireAssertion(c => Has(c, ReadOperations)));
    }
    private static bool Has(AuthorizationHandlerContext context, string permission)
    {
        if (context.User.IsInRole("operator") || context.User.IsInRole("admin")) return true;
        return context.User.FindAll("scp").SelectMany(c => c.Value.Split(' ',
            StringSplitOptions.RemoveEmptyEntries)).Contains(permission, StringComparer.Ordinal);
    }
}
