using AiInsights.Web;
using Xunit;

namespace AiInsights.Tests;

public sealed class InsightAnalyzerTests
{
    [Fact]
    public void AnalyzeReturnsEvidenceAndNeverSharesRowsExternally()
    {
        var report = new ParsedReport(
        [
            new ReportRow(2, new DateOnly(2026, 1, 1), "Sales", "Core", 100, 110, 40, 0),
            new ReportRow(3, new DateOnly(2026, 1, 2), "Support", "Core", 100, 60, 55, 12)
        ],
        ["Date", "Department", "Product", "Plan", "Actual", "Cost", "DelayDays", "ClientEmail"],
        ["ClientEmail"]);

        var result = new InsightAnalyzer().Analyze(report);

        Assert.Equal(2, result.Audit.RowsProcessed);
        Assert.Equal(0, result.Audit.RowsSharedExternally);
        Assert.Contains(result.SourceRows, row => row.SourceRow == 3);
        Assert.Contains("ClientEmail", result.Audit.RedactedColumns);
    }

    [Fact]
    public void AnalyzeGroupsMetricsByDepartment()
    {
        var report = new ParsedReport(
        [
            new ReportRow(2, new DateOnly(2026, 1, 1), "Sales", "Core", 100, 110, 40, 0),
            new ReportRow(3, new DateOnly(2026, 1, 2), "Sales", "Reports", 100, 90, 35, 1)
        ],
        ["Date", "Department", "Product", "Plan", "Actual", "Cost", "DelayDays"],
        []);

        var result = new InsightAnalyzer().Analyze(report);

        var metric = Assert.Single(result.DepartmentMetrics);
        Assert.Equal(200, metric.Plan);
        Assert.Equal(200, metric.Actual);
        Assert.Equal(100, metric.AttainmentPercent);
    }
}
