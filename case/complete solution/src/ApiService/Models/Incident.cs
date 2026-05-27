namespace AgenticIncidentService.ApiService.Models;

public record Incident(
    string Id,
    string Title,
    Severity Severity,
    string System,
    string Description,
    IReadOnlyList<string> Tags,
    DateTimeOffset ObservedAt,
    string Source);

public enum Severity
{
    Low,
    Medium,
    High,
    Critical
}
