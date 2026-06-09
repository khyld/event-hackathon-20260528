using AgenticIncidentService.ApiService;
using Xunit;

namespace AgenticIncidentService.UnitTests;

public class RecommendationEngineTests
{
    private static IncidentRecord MakeIncident(
        string category = "Availability",
        string priority = "P1",
        string severity = "High",
        string system = "Payments",
        string id = "INC-001") =>
        new(id, "Test incident", severity, system, "Test description",
            new[] { "test" }, "2026-05-20T09:12:00Z", category, priority, "Test reason", "synthetic");

    // --- Category → summary mapping ---

    [Fact]
    public void Availability_SummaryMentionsInvestigateDependency()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Availability", system: "Payments"));
        Assert.Contains("payments", rec.Summary);
        Assert.Contains("dependency", rec.Summary);
    }

    [Fact]
    public void Performance_SummaryMentionsPerformanceMetrics()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Performance", system: "Identity"));
        Assert.Contains("identity", rec.Summary);
        Assert.Contains("performance metrics", rec.Summary);
    }

    [Fact]
    public void Security_SummaryMentionsAccessLogs()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Security", system: "Auth"));
        Assert.Contains("auth", rec.Summary);
        Assert.Contains("access logs", rec.Summary);
    }

    // --- Category → nextAction mapping ---

    [Fact]
    public void Availability_NextActionMentionsGatewayLogs()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Availability"));
        Assert.Contains("gateway error logs", rec.NextAction);
    }

    [Fact]
    public void Performance_NextActionMentionsLatency()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Performance"));
        Assert.Contains("p95/p99 latency", rec.NextAction);
    }

    [Fact]
    public void Security_NextActionMentionsAudit()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Security"));
        Assert.Contains("authentication logs", rec.NextAction);
    }

    // --- Priority → confidence mapping ---

    [Theory]
    [InlineData("P0", "High")]
    [InlineData("P1", "High")]
    [InlineData("P2", "Medium")]
    [InlineData("P3", "Medium")]
    [InlineData("P4", "Medium")]
    public void Priority_MapsToExpectedConfidence(string priority, string expectedConfidence)
    {
        var rec = RecommendationEngine.Generate(MakeIncident(priority: priority));
        Assert.Equal(expectedConfidence, rec.Confidence);
    }

    // --- System name is lowercased in output ---

    [Fact]
    public void SystemName_IsLowercasedInSummaryAndNextAction()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(system: "Payments"));
        Assert.Contains("payments", rec.Summary);
        Assert.Contains("payments", rec.NextAction);
        Assert.DoesNotContain("Payments", rec.Summary);
    }

    // --- IncidentId passthrough ---

    [Fact]
    public void IncidentId_IsPassedThrough()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(id: "INC-042"));
        Assert.Equal("INC-042", rec.IncidentId);
    }

    // --- Determinism ---

    [Fact]
    public void SameInput_ProducesIdenticalOutput()
    {
        var incident = MakeIncident();
        var r1 = RecommendationEngine.Generate(incident);
        var r2 = RecommendationEngine.Generate(incident);

        Assert.Equal(r1.Summary, r2.Summary);
        Assert.Equal(r1.NextAction, r2.NextAction);
        Assert.Equal(r1.Confidence, r2.Confidence);
        Assert.Equal(r1.IncidentId, r2.IncidentId);
    }

    // --- Edge cases ---

    [Fact]
    public void UnknownCategory_FallsBackToGenericRecommendation()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: "Compliance"));
        Assert.Contains("Gather logs and metrics", rec.Summary);
        Assert.Contains("narrow the blast radius", rec.NextAction);
    }

    [Fact]
    public void EmptyCategory_FallsBackToGenericRecommendation()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(category: ""));
        Assert.Contains("Gather logs and metrics", rec.Summary);
    }

    [Fact]
    public void UnknownPriority_MapsToMediumConfidence()
    {
        var rec = RecommendationEngine.Generate(MakeIncident(priority: "Unknown"));
        Assert.Equal("Medium", rec.Confidence);
    }

    [Fact]
    public void AllFourFieldsArePopulated()
    {
        var rec = RecommendationEngine.Generate(MakeIncident());
        Assert.False(string.IsNullOrWhiteSpace(rec.IncidentId));
        Assert.False(string.IsNullOrWhiteSpace(rec.Summary));
        Assert.False(string.IsNullOrWhiteSpace(rec.NextAction));
        Assert.False(string.IsNullOrWhiteSpace(rec.Confidence));
    }
}
