namespace AgenticIncidentService.ApiService.Models;

public record TriageResult(
    string Category,
    string Priority,
    string Reason);
