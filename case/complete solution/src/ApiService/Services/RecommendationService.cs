using AgenticIncidentService.ApiService.Models;

namespace AgenticIncidentService.ApiService.Services;

public class RecommendationService
{
    public FixRecommendation Recommend(Incident incident, TriageResult triage)
    {
        var nextAction = triage.Category switch
        {
            "Payments" => "Check API gateway logs, retry policy, and downstream health. Roll back recent deploy if needed.",
            "Identity" => "Inspect DB and cache metrics. Verify recent auth configuration changes.",
            _ => "Collect logs/metrics, identify recent changes, and narrow the blast radius."
        };

        var confidence = triage.Priority is "P0" or "P1" ? "Medium" : "Low";

        return new FixRecommendation(
            IncidentId: incident.Id,
            Summary: $"{triage.Priority} / {triage.Category}: {triage.Reason}",
            NextAction: nextAction,
            Confidence: confidence);
    }
}
