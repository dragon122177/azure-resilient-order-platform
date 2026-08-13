using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace OrderGrid.Api.Tests;
public sealed class ApiSmokeTests(OrderGridApiFactory factory) : IClassFixture<OrderGridApiFactory>
{
    [Fact] public async Task Readiness_is_healthy()
    { using var client = factory.CreateClient(); Assert.Equal(HttpStatusCode.OK,
        (await client.GetAsync("/health/ready")).StatusCode); }

    [Fact] public async Task Entra_protects_business_endpoints_but_not_health()
    { using var f = new EntraOrderGridApiFactory(); using var client = f.CreateClient();
      Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
      Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/orders?page=1&pageSize=20")).StatusCode); }

    [Fact] public async Task Create_is_idempotent()
    { using var client = factory.CreateClient(); var payload = new
      { externalReference = $"API-{Guid.NewGuid():N}", customerEmail = "customer@example.com", currency = "JPY",
        shippingAddress = new { recipient = "Aiko", line1 = "1 Shibuya", city = "Tokyo", postalCode = "150-0002", countryCode = "JP" },
        items = new[] { new { sku = "AZ-100", name = "Book", quantity = 1, unitPrice = 3200m } } };
      var key = $"api-test-{Guid.NewGuid():N}";
      async Task<HttpResponseMessage> Send() { var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders");
        request.Headers.Add("Idempotency-Key", key); request.Content = JsonContent.Create(payload); return await client.SendAsync(request); }
      using var first = await Send(); using var second = await Send();
      Assert.Equal(HttpStatusCode.Created, first.StatusCode); Assert.Equal("true", second.Headers.GetValues("Idempotency-Replayed").Single());
      var a = await first.Content.ReadFromJsonAsync<JsonElement>(); var b = await second.Content.ReadFromJsonAsync<JsonElement>();
      Assert.Equal(a.GetProperty("id").GetString(), b.GetProperty("id").GetString()); }

    [Fact] public async Task Malformed_json_returns_safe_bad_request()
    { using var client = factory.CreateClient(); var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders");
      request.Headers.Add("Idempotency-Key", "malformed-json-test"); request.Content = new StringContent("{not-json", null, "application/json");
      var response = await client.SendAsync(request); var body = await response.Content.ReadAsStringAsync();
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); Assert.DoesNotContain("JsonException", body); }
}
