using System.Net;
using System.Net.Http.Json;
using System.Text;
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

    // --- GET /health ---

    [Fact]
    public async Task Health_Returns200WithStatusOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ok", body.GetProperty("status").GetString());
    }

    // --- GET /api/incidents (list) ---

    [Fact]
    public async Task GetIncidents_Returns200WithArray()
    {
        var response = await _client.GetAsync("/api/incidents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        Assert.True(body.GetArrayLength() >= 2, "Expected at least 2 incidents from seed data");
    }

    [Fact]
    public async Task GetIncidents_EachItemHasIdAndTitle()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/incidents");
        foreach (var item in body.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("id", out _), "Missing 'id' field");
            Assert.True(item.TryGetProperty("title", out _), "Missing 'title' field");
        }
    }

    // --- POST /api/incidents ---

    [Fact]
    public async Task PostIncident_ValidRequest_Returns201Created()
    {
        var payload = new { title = "New test incident", severity = "High", system = "Billing", description = "Test description" };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("id", out var id));
        Assert.StartsWith("INC-", id.GetString());
        Assert.Equal("New test incident", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task PostIncident_ValidRequest_AppearsInSubsequentList()
    {
        var payload = new { title = "Trackable incident", severity = "Low", system = "Audit", description = "For list check" };
        var postResponse = await _client.PostAsJsonAsync("/api/incidents", payload);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var list = await _client.GetFromJsonAsync<JsonElement>("/api/incidents");
        var titles = list.EnumerateArray().Select(e => e.GetProperty("title").GetString()).ToArray();
        Assert.Contains("Trackable incident", titles);
    }

    [Fact]
    public async Task PostIncident_MissingTitle_Returns400WithErrors()
    {
        var payload = new { title = "", severity = "High", system = "Billing", description = "Desc" };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("title", out _));
    }

    [Fact]
    public async Task PostIncident_InvalidSeverity_Returns400WithErrors()
    {
        var payload = new { title = "Test", severity = "Extreme", system = "Billing", description = "Desc" };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("severity", out _));
    }

    [Fact]
    public async Task PostIncident_MissingMultipleFields_Returns400WithAllErrors()
    {
        var payload = new { title = "", severity = "bogus", system = "", description = "" };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("system", out _));
        Assert.True(errors.TryGetProperty("description", out _));
        Assert.True(errors.TryGetProperty("severity", out _));
    }

    [Fact]
    public async Task PostIncident_WithTags_TagsArePreserved()
    {
        var payload = new { title = "Tagged incident", severity = "Medium", system = "Core", description = "Has tags", tags = new[] { "network", "dns" } };
        var postResponse = await _client.PostAsJsonAsync("/api/incidents", payload);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdBody = await postResponse.Content.ReadFromJsonAsync<JsonElement>();
        var createdId = createdBody.GetProperty("id").GetString();

        // Verify the incident is in the list
        var list = await _client.GetFromJsonAsync<JsonElement>("/api/incidents");
        var ids = list.EnumerateArray().Select(e => e.GetProperty("id").GetString()).ToArray();
        Assert.Contains(createdId, ids);
    }

    // --- Input length limit tests ---

    [Fact]
    public async Task PostIncident_TitleTooLong_Returns400()
    {
        var payload = new { title = new string('A', 201), severity = "High", system = "Billing", description = "Desc" };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("title", out _));
    }

    [Fact]
    public async Task PostIncident_DescriptionTooLong_Returns400()
    {
        var payload = new { title = "Test", severity = "High", system = "Billing", description = new string('B', 2001) };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("description", out _));
    }

    [Fact]
    public async Task PostIncident_TooManyTags_Returns400()
    {
        var tags = Enumerable.Range(0, 11).Select(i => $"tag{i}").ToArray();
        var payload = new { title = "Test", severity = "High", system = "Billing", description = "Desc", tags };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("tags", out _));
    }

    [Fact]
    public async Task PostIncident_TagTooLong_Returns400()
    {
        var payload = new { title = "Test", severity = "High", system = "Billing", description = "Desc", tags = new[] { new string('C', 51) } };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("tags", out _));
    }

    [Fact]
    public async Task PostIncident_FieldsAtMaxLength_Returns201()
    {
        var payload = new { title = new string('A', 200), severity = "Low", system = new string('B', 100), description = new string('C', 2000) };
        var response = await _client.PostAsJsonAsync("/api/incidents", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // --- Error message sanitization tests ---

    [Fact]
    public async Task GetIncident_InvalidId_ErrorDoesNotEchoInput()
    {
        var maliciousId = "EVIL-injection-payload";
        var response = await _client.GetAsync($"/api/incidents/{maliciousId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("EVIL", body);
        Assert.DoesNotContain("injection", body);
    }

    [Fact]
    public async Task GetIncident_UnknownId_ErrorDoesNotEchoInput()
    {
        var response = await _client.GetAsync("/api/incidents/INC-999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("INC-999", body);
    }
}
