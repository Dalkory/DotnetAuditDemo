using AiInsights.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ReportReader>();
builder.Services.AddSingleton<InsightAnalyzer>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/sample.xlsx", () =>
    Results.File(
        SampleReport.CreateXlsx(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ai-insights-synthetic-report.xlsx"));

app.MapGet("/api/sample.csv", () =>
    Results.File(SampleReport.CreateCsv(), "text/csv; charset=utf-8", "ai-insights-synthetic-report.csv"));

app.MapPost("/api/analyze", async (
    IFormFile file,
    ReportReader reader,
    InsightAnalyzer analyzer,
    CancellationToken cancellationToken) =>
{
    if (file.Length == 0)
    {
        return Results.BadRequest(new { error = "Файл пуст." });
    }

    if (file.Length > 15 * 1024 * 1024)
    {
        return Results.BadRequest(new { error = "Демо принимает файлы до 15 МБ." });
    }

    try
    {
        var report = await reader.ReadAsync(file, cancellationToken);
        return Results.Ok(analyzer.Analyze(report));
    }
    catch (InvalidDataException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
}).DisableAntiforgery();

app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;
