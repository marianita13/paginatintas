using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.Hosting;

namespace API.Tests;

public sealed class DeploymentTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient client;

    public DeploymentTests(WebApplicationFactory<global::Program> factory)
    {
        client = factory.CreateClient();    
    }

    [Fact]
    public async Task HealthEndpointReturnsOkStatus()
    {
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    private sealed record HealthPayload(string Status);
}
