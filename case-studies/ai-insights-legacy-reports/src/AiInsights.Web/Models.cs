namespace AiInsights.Web;

public sealed record ReportRow(
    int SourceRow,
    DateOnly Date,
    string Department,
    string Product,
    decimal Plan,
    decimal Actual,
    decimal Cost,
    int DelayDays);

public sealed record SourceEvidence(
    int SourceRow,
    string Department,
    string Product,
    string Reason,
    decimal Plan,
    decimal Actual,
    int DelayDays);

public sealed record DepartmentMetric(
    string Department,
    decimal Plan,
    decimal Actual,
    decimal Variance,
    decimal AttainmentPercent,
    decimal Cost);

public sealed record InsightAudit(
    string AnalysisVersion,
    string NarrativeMode,
    string DataBoundary,
    int RowsProcessed,
    int RowsSharedExternally,
    IReadOnlyList<string> DetectedColumns,
    IReadOnlyList<string> RedactedColumns);

public sealed record InsightsResponse(
    string Summary,
    IReadOnlyList<string> Anomalies,
    IReadOnlyList<string> Trends,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> RecommendedQuestions,
    IReadOnlyList<SourceEvidence> SourceRows,
    IReadOnlyList<DepartmentMetric> DepartmentMetrics,
    InsightAudit Audit);

