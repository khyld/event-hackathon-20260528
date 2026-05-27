using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AgenticIncidentService.IntegrationTests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Incidents_ReturnsOkAndList()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/incidents");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("INC-", json);
    }

    [Fact]
    public async Task CreateIncident_ReturnsCreated()
    {
        var client = _factory.CreateClient();

        var payload = new {
            title = "New incident",
            severity = "Low",
            system = "General",
            description = "Something happened",
            tags = new []{ "demo" }
        };

        var response = await client.PostAsJsonAsync("/api/incidents", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
