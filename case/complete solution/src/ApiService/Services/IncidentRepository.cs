using System.Text.Json;
using AgenticIncidentService.ApiService.Models;

namespace AgenticIncidentService.ApiService.Services;

public class IncidentRepository : IIncidentRepository
{
    private readonly object _lock = new();
    private readonly List<Incident> _items;

    public IncidentRepository()
    {
        _items = LoadSeed();
    }

    public IReadOnlyList<Incident> GetAll()
    {
        lock (_lock)
        {
            return _items.ToList();
        }
    }

    public Incident? GetById(string id)
    {
        lock (_lock)
        {
            return _items.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Incident Add(CreateIncidentRequest request)
    {
        var sev = Enum.Parse<Severity>(request.Severity, true);
        var incident = new Incident(
            Id: $"INC-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Title: request.Title.Trim(),
            Severity: sev,
            System: request.System.Trim(),
            Description: request.Description.Trim(),
            Tags: (request.Tags ?? Array.Empty<string>()).Select(t => t.Trim()).Where(t => t.Length > 0).ToArray(),
            ObservedAt: DateTimeOffset.UtcNow,
            Source: "user"
        );

        lock (_lock)
        {
            _items.Add(incident);
        }

        return incident;
    }

    private static List<Incident> LoadSeed()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "data", "incidents.json");
            if (!File.Exists(path)) return new();

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Seed file uses lowercase strings for severity; normalize.
            var raw = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, options) ?? new();
            var result = new List<Incident>();

            foreach (var item in raw)
            {
                var id = item["id"]?.ToString() ?? Guid.NewGuid().ToString("N");
                var title = item["title"]?.ToString() ?? "(untitled)";
                var severityStr = item["severity"]?.ToString() ?? "Low";
                var system = item["system"]?.ToString() ?? "Unknown";
                var description = item["description"]?.ToString() ?? "";
                var tags = new List<string>();
                if (item.TryGetValue("tags", out var tv) && tv is JsonElement je && je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in je.EnumerateArray()) tags.Add(t.GetString() ?? "");
                }
                var observedAt = DateTimeOffset.TryParse(item["observedAt"]?.ToString(), out var o) ? o : DateTimeOffset.UtcNow;
                var source = item["source"]?.ToString() ?? "seed";

                Enum.TryParse<Severity>(severityStr, true, out var sev);

                result.Add(new Incident(id, title, sev, system, description, tags, observedAt, source));
            }

            return result;
        }
        catch
        {
            return new();
        }
    }
}
