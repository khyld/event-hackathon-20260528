namespace AgenticIncidentService.ApiService.Models;

public record CreateIncidentRequest(
    string Title,
    string Severity,
    string System,
    string Description,
    IReadOnlyList<string>? Tags)
{
    public static Dictionary<string, string> Validate(CreateIncidentRequest r)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(r.Title)) errors["title"] = "Title is required";
        if (string.IsNullOrWhiteSpace(r.System)) errors["system"] = "System is required";
        if (string.IsNullOrWhiteSpace(r.Description)) errors["description"] = "Description is required";

        if (!Enum.TryParse<Severity>(r.Severity, true, out _))
            errors["severity"] = "Severity must be one of: Low, Medium, High, Critical";

        return errors;
    }
}
