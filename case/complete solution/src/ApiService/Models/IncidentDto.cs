namespace AgenticIncidentService.ApiService.Models;

public record IncidentDto(
    string Id,
    string Title,
    string Severity,
    string System,
    IReadOnlyList<string> Tags,
    DateTimeOffset ObservedAt,
    string Category,
    string Priority)
{
    public static IncidentDto From(Incident i, TriageResult triage) =>
        new(i.Id, i.Title, i.Severity.ToString(), i.System, i.Tags, i.ObservedAt, triage.Category, triage.Priority);
}

public record IncidentDetailsDto(
    string Id,
    string Title,
    string Severity,
    string System,
    string Description,
    IReadOnlyList<string> Tags,
    DateTimeOffset ObservedAt,
    string Category,
    string Priority,
    string Reason)
{
    public static IncidentDetailsDto From(Incident i, TriageResult triage) =>
        new(i.Id, i.Title, i.Severity.ToString(), i.System, i.Description, i.Tags, i.ObservedAt, triage.Category, triage.Priority, triage.Reason);
}
