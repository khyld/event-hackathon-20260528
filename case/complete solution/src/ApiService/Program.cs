using AgenticIncidentService.ApiService.Models;
using AgenticIncidentService.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IIncidentRepository, IncidentRepository>();
builder.Services.AddSingleton<TriageService>();
builder.Services.AddSingleton<RecommendationService>();

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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/incidents", (IIncidentRepository repo, TriageService triage) =>
{
    var items = repo.GetAll().Select(i => IncidentDto.From(i, triage.Classify(i)));
    return Results.Ok(items);
});

app.MapGet("/api/incidents/{id}", (string id, IIncidentRepository repo, TriageService triage) =>
{
    var incident = repo.GetById(id);
    return incident is null
        ? Results.NotFound()
        : Results.Ok(IncidentDetailsDto.From(incident, triage.Classify(incident)));
});

app.MapPost("/api/incidents", (CreateIncidentRequest request, IIncidentRepository repo) =>
{
    var validation = CreateIncidentRequest.Validate(request);
    if (validation.Count > 0)
    {
        return Results.BadRequest(new { errors = validation });
    }

    var created = repo.Add(request);
    return Results.Created($"/api/incidents/{created.Id}", created);
});

app.MapGet("/api/incidents/{id}/recommendation", (string id, IIncidentRepository repo, TriageService triage, RecommendationService recs) =>
{
    var incident = repo.GetById(id);
    if (incident is null) return Results.NotFound();

    var triageResult = triage.Classify(incident);
    var recommendation = recs.Recommend(incident, triageResult);
    return Results.Ok(recommendation);
});

app.Run();

public partial class Program { }
