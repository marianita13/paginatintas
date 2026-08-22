using System.Net.Http;
using System.Threading.Tasks;
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
        response.EnsureSuccessStatusCode();
    }

    private sealed record HealthPayload(string Status);
}
