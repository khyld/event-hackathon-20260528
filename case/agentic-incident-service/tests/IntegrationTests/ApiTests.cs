using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AgenticIncidentService.IntegrationTests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Test 1: Valid ID returns incident detail (200, 10 fields)
    [Fact]
    public async Task GetIncident_ValidId_Returns200WithExactly10Fields()
    {
        var response = await _client.GetAsync("/api/incidents/INC-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var expectedFields = new[] { "id", "title", "severity", "system", "tags", "observedAt", "category", "priority", "description", "reason" };
        foreach (var field in expectedFields)
            Assert.True(body.TryGetProperty(field, out _), $"Missing field: {field}");
        Assert.Equal(10, CountProperties(body));
        Assert.Equal("INC-001", body.GetProperty("id").GetString());
    }

    // Test 2: Valid ID returns recommendation (200, 4 fields)
    [Fact]
    public async Task GetRecommendation_ValidId_Returns200WithExactly4Fields()
    {
        var response = await _client.GetAsync("/api/incidents/INC-001/recommendation");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var expectedFields = new[] { "incidentId", "summary", "nextAction", "confidence" };
        foreach (var field in expectedFields)
            Assert.True(body.TryGetProperty(field, out _), $"Missing field: {field}");
        Assert.Equal(4, CountProperties(body));
        Assert.Equal("INC-001", body.GetProperty("incidentId").GetString());
    }

    // Test 3: Invalid format ID on detail → 400
    [Theory]
    [InlineData("abc")]
    [InlineData("INC001")]
    [InlineData("inc-001")]
    [InlineData("INC-")]
    [InlineData("INC-0001")]
    public async Task GetIncident_InvalidId_Returns400(string id)
    {
        var response = await _client.GetAsync($"/api/incidents/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out _));
        Assert.Equal(400, body.GetProperty("status").GetInt32());
    }

    // Test 4: Invalid format ID on recommendation → 400
    [Theory]
    [InlineData("abc")]
    [InlineData("INC001")]
    [InlineData("inc-001")]
    public async Task GetRecommendation_InvalidId_Returns400(string id)
    {
        var response = await _client.GetAsync($"/api/incidents/{id}/recommendation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out _));
        Assert.Equal(400, body.GetProperty("status").GetInt32());
    }

    // Test 5: Unknown ID (INC-999) on detail → 404
    [Fact]
    public async Task GetIncident_UnknownId_Returns404()
    {
        var response = await _client.GetAsync("/api/incidents/INC-999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out _));
        Assert.Equal(404, body.GetProperty("status").GetInt32());
    }

    // Test 6: Unknown ID (INC-999) on recommendation → 404
    [Fact]
    public async Task GetRecommendation_UnknownId_Returns404()
    {
        var response = await _client.GetAsync("/api/incidents/INC-999/recommendation");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out _));
        Assert.Equal(404, body.GetProperty("status").GetInt32());
    }

    // Test 7: Repeated recommendation calls return identical output
    [Fact]
    public async Task GetRecommendation_RepeatedCalls_ReturnIdenticalOutput()
    {
        var r1 = await _client.GetStringAsync("/api/incidents/INC-001/recommendation");
        var r2 = await _client.GetStringAsync("/api/incidents/INC-001/recommendation");

        Assert.Equal(r1, r2);
    }

    // Test 8: Success responses have Content-Type application/json
    [Fact]
    public async Task SuccessResponses_HaveJsonContentType()
    {
        var detailResponse = await _client.GetAsync("/api/incidents/INC-001");
        Assert.Equal("application/json", detailResponse.Content.Headers.ContentType?.MediaType);

        var recResponse = await _client.GetAsync("/api/incidents/INC-001/recommendation");
        Assert.Equal("application/json", recResponse.Content.Headers.ContentType?.MediaType);

        var listResponse = await _client.GetAsync("/api/incidents");
        Assert.Equal("application/json", listResponse.Content.Headers.ContentType?.MediaType);
    }

    // Test 9: Error responses have Content-Type application/json
    [Fact]
    public async Task ErrorResponses_HaveJsonContentType()
    {
        var badRequest = await _client.GetAsync("/api/incidents/abc");
        Assert.Equal("application/json", badRequest.Content.Headers.ContentType?.MediaType);

        var notFound = await _client.GetAsync("/api/incidents/INC-999");
        Assert.Equal("application/json", notFound.Content.Headers.ContentType?.MediaType);
    }

    // Test 10: Incident with tags returns array, not null
    [Fact]
    public async Task GetIncident_TagsField_ReturnsArray()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-001");
        var tags = body.GetProperty("tags");
        Assert.Equal(JsonValueKind.Array, tags.ValueKind);
    }

    // Test 11: Incident reason field returns string, not null
    [Fact]
    public async Task GetIncident_ReasonField_ReturnsString()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-001");
        var reason = body.GetProperty("reason");
        Assert.Equal(JsonValueKind.String, reason.ValueKind);
    }

    // Test 12: Detail response has exactly 10 properties (no extra)
    [Fact]
    public async Task GetIncident_DetailResponse_HasExactly10Properties()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-002");
        Assert.Equal(10, CountProperties(body));
    }

    // Test 13: Recommendation response has exactly 4 properties (no extra)
    [Fact]
    public async Task GetRecommendation_Response_HasExactly4Properties()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-002/recommendation");
        Assert.Equal(4, CountProperties(body));
    }

    // Test 15: 5 concurrent recommendation calls return identical results
    [Fact]
    public async Task GetRecommendation_ConcurrentCalls_ReturnIdenticalResults()
    {
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetStringAsync("/api/incidents/INC-001/recommendation"));
        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.Equal(results[0], r));
    }

    // Test 19: Detail fields match IncidentDetails type in api.ts
    [Fact]
    public async Task GetIncident_FieldNames_MatchFrontendContract()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-001");
        var expected = new[] { "id", "title", "severity", "system", "tags", "observedAt", "category", "priority", "description", "reason" };
        var actual = body.EnumerateObject().Select(p => p.Name).OrderBy(n => n).ToArray();
        Assert.Equal(expected.OrderBy(n => n).ToArray(), actual);
    }

    // Test 20: Recommendation fields match Recommendation type in api.ts
    [Fact]
    public async Task GetRecommendation_FieldNames_MatchFrontendContract()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents/INC-001/recommendation");
        var expected = new[] { "incidentId", "summary", "nextAction", "confidence" };
        var actual = body.EnumerateObject().Select(p => p.Name).OrderBy(n => n).ToArray();
        Assert.Equal(expected.OrderBy(n => n).ToArray(), actual);
    }

    // List endpoint returns data from incidents.json
    [Fact]
    public async Task GetIncidents_ReturnsDataFromFile()
    {
        var response = await _client.GetAsync("/api/incidents");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("INC-001", json);
        Assert.Contains("INC-002", json);
    }

    private static int CountProperties(JsonElement element) =>
        element.EnumerateObject().Count();

    private async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}
