namespace AgenticIncidentService.ApiService.Models;

public record FixRecommendation(
    string IncidentId,
    string Summary,
    string NextAction,
    string Confidence);
