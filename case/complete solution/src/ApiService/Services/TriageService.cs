using AgenticIncidentService.ApiService.Models;

namespace AgenticIncidentService.ApiService.Services;

public class TriageService
{
    public TriageResult Classify(Incident incident)
    {
        var category = incident.System switch
        {
            var s when s.Contains("pay", StringComparison.OrdinalIgnoreCase) => "Payments",
            var s when s.Contains("ident", StringComparison.OrdinalIgnoreCase) => "Identity",
            _ => "General"
        };

        var (priority, reason) = incident.Severity switch
        {
            Severity.Critical => ("P0", "Critical severity"),
            Severity.High => ("P1", "High severity"),
            Severity.Medium => ("P2", "Medium severity"),
            _ => ("P3", "Low severity")
        };

        // Boost priority if certain keywords appear
        if (incident.Description.Contains("timeout", StringComparison.OrdinalIgnoreCase) && priority != "P0")
        {
            priority = "P1";
            reason = "Timeout symptoms";
        }

        return new TriageResult(category, priority, reason);
    }
}
