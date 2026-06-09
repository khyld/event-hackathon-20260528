using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticIncidentService.ApiService;

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
        return Results.Json(new { error = "Invalid incident ID format. Expected INC-NNN.", status = 400 }, statusCode: 400);
    return null!;
}

IResult? FindIncident(string id, out IncidentRecord? incident)
{
    incident = incidents.FirstOrDefault(i => i.Id == id);
    if (incident is null)
        return Results.Json(new { error = "Incident not found.", status = 404 }, statusCode: 404);
    return null;
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/incidents", () =>
    Results.Ok(incidents.Select(i => new { id = i.Id, title = i.Title })));

app.MapPost("/api/incidents", (CreateIncidentRequest request) =>
{
    var errors = new Dictionary<string, string>();
    if (string.IsNullOrWhiteSpace(request.Title)) errors["title"] = "Title is required";
    else if (request.Title.Length > 200) errors["title"] = "Title must be 200 characters or fewer";
    if (string.IsNullOrWhiteSpace(request.System)) errors["system"] = "System is required";
    else if (request.System.Length > 100) errors["system"] = "System must be 100 characters or fewer";
    if (string.IsNullOrWhiteSpace(request.Description)) errors["description"] = "Description is required";
    else if (request.Description.Length > 2000) errors["description"] = "Description must be 2000 characters or fewer";

    if (request.Tags is { Length: > 10 })
        errors["tags"] = "A maximum of 10 tags is allowed";
    else if (request.Tags?.Any(t => t.Length > 50) == true)
        errors["tags"] = "Each tag must be 50 characters or fewer";

    var validSeverities = new[] { "Low", "Medium", "High", "Critical" };
    if (!validSeverities.Contains(request.Severity, StringComparer.OrdinalIgnoreCase))
        errors["severity"] = "Severity must be one of: Low, Medium, High, Critical";

    if (errors.Count > 0)
        return Results.BadRequest(new { errors });

    var id = $"INC-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    var created = new IncidentRecord(
        Id: id,
        Title: request.Title.Trim(),
        Severity: request.Severity,
        System: request.System.Trim(),
        Description: request.Description.Trim(),
        Tags: (request.Tags ?? []).Select(t => t.Trim()).Where(t => t.Length > 0).ToArray(),
        ObservedAt: DateTimeOffset.UtcNow.ToString("o"),
        Category: "Unknown",
        Priority: "P3",
        Reason: null,
        Source: "user");

    incidents.Add(created);
    return Results.Created($"/api/incidents/{id}", new { id = created.Id, title = created.Title });
});

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

    var rec = RecommendationEngine.Generate(incident!);
    return Results.Ok(new
    {
        incidentId = rec.IncidentId,
        summary = rec.Summary,
        nextAction = rec.NextAction,
        confidence = rec.Confidence
    });
});

app.Run();

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

public record CreateIncidentRequest(
    string Title,
    string Severity,
    string System,
    string Description,
    string[]? Tags);
