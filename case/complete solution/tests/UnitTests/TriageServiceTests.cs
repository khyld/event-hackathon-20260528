using AgenticIncidentService.ApiService.Models;
using AgenticIncidentService.ApiService.Services;
using Xunit;

namespace AgenticIncidentService.UnitTests;

public class TriageServiceTests
{
    [Fact]
    public void HighSeverity_ProducesP1()
    {
        var triage = new TriageService();
        var incident = new Incident("INC", "t", Severity.High, "Payments", "", new []{ "x" }, DateTimeOffset.UtcNow, "test");

        var r = triage.Classify(incident);

        Assert.Equal("P1", r.Priority);
        Assert.Equal("Payments", r.Category);
    }

    [Fact]
    public void Timeout_BoostsToP1()
    {
        var triage = new TriageService();
        var incident = new Incident("INC", "t", Severity.Low, "General", "timeout", new []{ "x" }, DateTimeOffset.UtcNow, "test");

        var r = triage.Classify(incident);

        Assert.Equal("P1", r.Priority);
    }
}
