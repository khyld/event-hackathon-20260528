namespace AgenticIncidentService.ApiService;

public record Recommendation(
    string IncidentId,
    string Summary,
    string NextAction,
    string Confidence);

public static class RecommendationEngine
{
    public static Recommendation Generate(IncidentRecord incident)
    {
        var system = incident.System.ToLowerInvariant();

        var summary = incident.Category switch
        {
            "Availability" => $"Investigate the {system} dependency and recent deployment changes.",
            "Performance" => $"Analyze {system} performance metrics and recent configuration changes.",
            "Security" => $"Review {system} access logs and recent security events.",
            _ => $"Gather logs and metrics for {system} and identify recent changes."
        };

        var nextAction = incident.Category switch
        {
            "Availability" => $"Check gateway error logs, retry metrics, and the latest deployment for the {system} service.",
            "Performance" => $"Review p95/p99 latency, database query plans, and recent migrations for the {system} service.",
            "Security" => $"Audit authentication logs, token expiry, and access policy changes for the {system} service.",
            _ => $"Collect relevant telemetry and narrow the blast radius for the {system} service."
        };

        var confidence = incident.Priority is "P1" or "P0" ? "High" : "Medium";

        return new Recommendation(incident.Id, summary, nextAction, confidence);
    }
}
