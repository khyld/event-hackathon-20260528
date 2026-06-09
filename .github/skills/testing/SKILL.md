---
description: "Guidance for writing and structuring tests in the agentic-incident-service project."
---

# Testing Skill

## When to use

Use this skill when:

- Implementing a new API endpoint or service and need to add tests.
- A behavior change has no accompanying tests (the reviewer agent will flag this).
- You need to cover items from the spec's 20-test plan in `specs/incident-triage/spec.md`.
- You are unsure how to structure a test, which assertions to use, or where the test file belongs.

Do **not** use this skill for frontend-only component tests or end-to-end browser automation.

## Project layout

```
tests/
├── UnitTests/
│   ├── UnitTests.csproj          # References ApiService.csproj
│   └── <ServiceName>Tests.cs     # One file per service class
└── IntegrationTests/
    ├── IntegrationTests.csproj   # References ApiService.csproj + Mvc.Testing
    └── ApiTests.cs               # HTTP-level tests against the real pipeline
```

Both test projects must include the data file so the API can load it at test time:

```xml
<Content Include="../../data/incidents.json">
  <Link>data/incidents.json</Link>
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>
```

## Tech stack

| Concern | Choice |
|---|---|
| Framework | xUnit (`[Fact]`, `[Theory]`, `[InlineData]`) |
| Test SDK | `Microsoft.NET.Test.Sdk` |
| Integration host | `Microsoft.AspNetCore.Mvc.Testing` via `WebApplicationFactory<Program>` |
| Assertions | xUnit built-in (`Assert.Equal`, `Assert.Contains`, `Assert.Null`, etc.) |
| Target framework | Same as the API — `net10.0` |

## How to structure tests

### Unit tests

Unit tests target a single service class with no HTTP pipeline. Create the service directly.

```csharp
public class TriageServiceTests
{
    [Fact]
    public void HighSeverity_ProducesP1()
    {
        var service = new TriageService();
        var incident = BuildIncident(severity: Severity.High, system: "Payments");

        var result = service.Classify(incident);

        Assert.Equal("P1", result.Priority);
    }
}
```

Rules:
- One `[Fact]` per behavior. Name the method `<Condition>_<ExpectedOutcome>`.
- Use `[Theory]` + `[InlineData]` when the same logic is tested across multiple inputs.
- Do not call the network, file system, or clock. If the code under test needs data, pass it in.
- Keep arrange/act/assert blocks visually separated.

### Integration tests

Integration tests send real HTTP requests through the ASP.NET Core pipeline using `WebApplicationFactory`.

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetIncident_ValidId_Returns200WithExpectedFields()
    {
        var response = await _client.GetAsync("/api/incidents/INC-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INC-001", body.GetProperty("id").GetString());
    }
}
```

Rules:
- Use `IClassFixture<WebApplicationFactory<Program>>` so the host is shared across tests in the class.
- Assert status code first, then Content-Type, then body.
- Deserialize to `JsonElement` for strict schema checks (no missing/extra field surprises from a typed model).
- Never assert against hardcoded example text from the spec — assert against the actual data in `data/incidents.json`.

## Required test categories

The spec defines 20 required tests. Map them to code like this:

### Happy path (tests 1–2)
- `GET /api/incidents/{id}` → 200, body has exactly the 10 `IncidentDetails` fields.
- `GET /api/incidents/{id}/recommendation` → 200, body has exactly the 4 `Recommendation` fields.

### Validation errors (tests 3–6)
- Invalid format ID (`abc`, `INC001`, `inc-001`, empty string) → 400 with `{ "error": "...", "status": 400 }`.
- Valid format but missing (`INC-999`) → 404 with `{ "error": "...", "status": 404 }`.
- Test both the detail and recommendation endpoints separately.

### Determinism (tests 7, 14–15)
- Call recommendation twice in sequence → identical `summary`, `nextAction`, `confidence`.
- Restart the `WebApplicationFactory`, call again → still identical.
- Fire 5 concurrent requests → all responses match.

### Content-Type (tests 8–9)
- Every success response header: `application/json`.
- Every error response header: `application/json`.

### Null safety (tests 10–11)
- Incident with empty `tags` → response has `[]`, not `null`.
- Incident with missing `reason` → response has `""`, not `null`.

### Strict schema (tests 12–13, 19–20)
- Detail response has exactly 10 properties — no extra keys.
- Recommendation response has exactly 4 properties — no extra keys.
- Field names match the TypeScript types in `src/WebApp/src/api.ts`.

## How to handle edge cases

### Invalid input
Always test both sides of the validation boundary:

```csharp
[Theory]
[InlineData("abc")]          // not an ID at all
[InlineData("INC001")]       // missing hyphen
[InlineData("inc-001")]      // wrong case
[InlineData("")]             // empty
[InlineData("INC-")]         // incomplete
[InlineData("INC-0001")]     // too many digits
public async Task GetIncident_InvalidId_Returns400(string id)
{
    var response = await _client.GetAsync($"/api/incidents/{id}");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    Assert.True(body.TryGetProperty("error", out _));
    Assert.Equal(400, body.GetProperty("status").GetInt32());
}
```

### Missing data fields
Add a test incident to `data/incidents.json` with intentionally empty `tags` and absent `reason` (or create a fixture in test setup) to exercise null-coalescing paths. The API must return `[]` and `""`, never `null` or omit the field.

### Concurrency
Use `Task.WhenAll` to fire parallel requests and assert all responses are byte-identical:

```csharp
[Fact]
public async Task Recommendation_ConcurrentCalls_ReturnIdenticalResults()
{
    var tasks = Enumerable.Range(0, 5)
        .Select(_ => _client.GetStringAsync("/api/incidents/INC-001/recommendation"));
    var results = await Task.WhenAll(tasks);

    Assert.All(results, r => Assert.Equal(results[0], r));
}
```

### Trailing slashes and extra segments
Decide on a convention (the spec doesn't require support) and test that the API returns 404 for unexpected paths like `/api/incidents/INC-001/recommendation/extra`.

## Running tests

```bash
# From the repo root
cd case/agentic-incident-service

# All tests
dotnet test

# Unit tests only
dotnet test tests/UnitTests

# Integration tests only
dotnet test tests/IntegrationTests

# Single test by name
dotnet test --filter "GetIncident_ValidId_Returns200WithExpectedFields"
```

## Checklist before submitting

- [ ] Every new endpoint has at least: happy path, 400, 404, and Content-Type tests.
- [ ] Tests use data from `incidents.json`, not hardcoded spec examples.
- [ ] No test depends on execution order or shared mutable state.
- [ ] `dotnet test` passes with zero warnings in both projects.
- [ ] Test names follow `<Condition>_<ExpectedOutcome>` convention.
