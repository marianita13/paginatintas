using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace API.Tests;

public sealed class DeploymentTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient client;

    public DeploymentTests(WebApplicationFactory<global::Program> factory)
    {
        client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    [Fact]
    public async Task HealthEndpointReturnsOkStatus()
    {
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>();
        Assert.Equal("ok", payload?.Status);
    }

    private sealed record HealthPayload(string Status);
}
