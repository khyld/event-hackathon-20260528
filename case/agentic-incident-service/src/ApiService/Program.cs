using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// NOTE: Wide-open CORS is for local development only.
// Restrict origins before deploying to any shared or production environment.
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

// Load incidents from data/incidents.json (single source of truth)
var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "incidents.json");
var incidents = new List<IncidentRecord>();
if (File.Exists(dataPath))
{
    var json = File.ReadAllText(dataPath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    incidents = JsonSerializer.Deserialize<List<IncidentRecord>>(json, options) ?? [];
}

var idPattern = new Regex(@"^INC-\d{3}$", RegexOptions.Compiled);

IResult ValidateId(string id)
{
    if (!idPattern.IsMatch(id))
        return Results.Json(new { error = $"Invalid incident ID format: '{id}'. Expected INC-NNN.", status = 400 }, statusCode: 400);
    return null!;
}

IResult? FindIncident(string id, out IncidentRecord? incident)
{
    incident = incidents.FirstOrDefault(i => i.Id == id);
    if (incident is null)
        return Results.Json(new { error = $"Incident '{id}' not found.", status = 404 }, statusCode: 404);
    return null;
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/incidents", () =>
    Results.Ok(incidents.Select(i => new { id = i.Id, title = i.Title })));

app.MapGet("/api/incidents/{id}", (string id) =>
{
    var invalid = ValidateId(id);
    if (invalid is not null) return invalid;

    var notFound = FindIncident(id, out var incident);
    if (notFound is not null) return notFound;

    return Results.Ok(new
    {
        id = incident!.Id,
        title = incident.Title,
        severity = incident.Severity,
        system = incident.System,
        tags = incident.Tags ?? [],
        observedAt = incident.ObservedAt,
        category = incident.Category,
        priority = incident.Priority,
        description = incident.Description,
        reason = incident.Reason ?? ""
    });
});

app.MapGet("/api/incidents/{id}/recommendation", (string id) =>
{
    var invalid = ValidateId(id);
    if (invalid is not null) return invalid;

    var notFound = FindIncident(id, out var incident);
    if (notFound is not null) return notFound;

    return Results.Ok(GenerateRecommendation(incident!));
});

app.Run();

// Deterministic recommendation — pure function of the incident record.
static object GenerateRecommendation(IncidentRecord incident)
{
    var summary = incident.Category switch
    {
        "Availability" => $"Investigate the {incident.System.ToLowerInvariant()} dependency and recent deployment changes.",
        "Performance" => $"Analyze {incident.System.ToLowerInvariant()} performance metrics and recent configuration changes.",
        "Security" => $"Review {incident.System.ToLowerInvariant()} access logs and recent security events.",
        _ => $"Gather logs and metrics for {incident.System.ToLowerInvariant()} and identify recent changes."
    };

    var nextAction = incident.Category switch
    {
        "Availability" => $"Check gateway error logs, retry metrics, and the latest deployment for the {incident.System.ToLowerInvariant()} service.",
        "Performance" => $"Review p95/p99 latency, database query plans, and recent migrations for the {incident.System.ToLowerInvariant()} service.",
        "Security" => $"Audit authentication logs, token expiry, and access policy changes for the {incident.System.ToLowerInvariant()} service.",
        _ => $"Collect relevant telemetry and narrow the blast radius for the {incident.System.ToLowerInvariant()} service."
    };

    var confidence = incident.Priority is "P1" or "P0" ? "High" : "Medium";

    return new
    {
        incidentId = incident.Id,
        summary,
        nextAction,
        confidence
    };
}

public partial class Program { }

public record IncidentRecord(
    string Id,
    string Title,
    string Severity,
    string System,
    string Description,
    string[]? Tags,
    string ObservedAt,
    string Category,
    string Priority,
    string? Reason,
    string? Source);
