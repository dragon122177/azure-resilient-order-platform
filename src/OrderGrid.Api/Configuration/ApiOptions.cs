namespace OrderGrid.Api.Configuration;
public sealed class ApiOptions
{
    public const string SectionName = "Api";
    public string Name { get; init; } = "OrderGrid API";
    public string Version { get; init; } = "v1";
}
