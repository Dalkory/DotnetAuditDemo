using System.Globalization;

namespace AiInsights.Web;

public sealed class InsightAnalyzer
{
    public InsightsResponse Analyze(ParsedReport report)
    {
        var rows = report.Rows;
        var totalPlan = rows.Sum(row => row.Plan);
        var totalActual = rows.Sum(row => row.Actual);
        var totalCost = rows.Sum(row => row.Cost);
        var attainment = totalPlan == 0 ? 0 : totalActual / totalPlan * 100;

        var departmentMetrics = rows
            .GroupBy(row => row.Department, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var plan = group.Sum(row => row.Plan);
                var actual = group.Sum(row => row.Actual);
                return new DepartmentMetric(
                    group.Key,
                    plan,
                    actual,
                    actual - plan,
                    plan == 0 ? 0 : decimal.Round(actual / plan * 100, 1),
                    group.Sum(row => row.Cost));
            })
            .OrderBy(metric => metric.AttainmentPercent)
            .ToArray();

        var evidence = rows
            .Select(row => new
            {
                Row = row,
                Variance = row.Actual - row.Plan,
                Score = Math.Abs(row.Actual - row.Plan) + row.DelayDays * Math.Max(row.Plan, 1) / 10
            })
            .Where(item => item.Row.DelayDays >= 5 || item.Row.Actual < item.Row.Plan * 0.8m)
            .OrderByDescending(item => item.Score)
            .Take(8)
            .Select(item => new SourceEvidence(
                item.Row.SourceRow,
                item.Row.Department,
                item.Row.Product,
                item.Row.DelayDays >= 5
                    ? $"задержка {item.Row.DelayDays} дн.; отклонение {Money(item.Variance)}"
                    : $"выполнение {Percent(item.Row.Plan == 0 ? 0 : item.Row.Actual / item.Row.Plan * 100)}",
                item.Row.Plan,
                item.Row.Actual,
                item.Row.DelayDays))
            .ToArray();

        var orderedDates = rows.Select(row => row.Date).Distinct().Order().ToArray();
        var midpoint = orderedDates.Length == 0 ? default : orderedDates[orderedDates.Length / 2];
        var firstActual = rows.Where(row => row.Date < midpoint).Sum(row => row.Actual);
        var secondActual = rows.Where(row => row.Date >= midpoint).Sum(row => row.Actual);
        var trendPercent = firstActual == 0 ? 0 : (secondActual - firstActual) / firstActual * 100;
        var weakest = departmentMetrics.First();
        var highestCost = departmentMetrics.OrderByDescending(metric => metric.Cost).First();

        var anomalies = evidence
            .Take(5)
            .Select(item => $"Строка {item.SourceRow}: {item.Department} / {item.Product} — {item.Reason}.")
            .ToArray();
        var risks = new List<string>();
        if (attainment < 90)
        {
            risks.Add($"Общее выполнение плана ниже 90%: {Percent(attainment)}.");
        }
        if (rows.Any(row => row.DelayDays >= 10))
        {
            risks.Add($"Найдены {rows.Count(row => row.DelayDays >= 10)} строк с задержкой 10 дней и более.");
        }
        if (highestCost.Cost > totalCost * 0.4m)
        {
            risks.Add($"На {highestCost.Department} приходится более 40% всех затрат.");
        }
        if (risks.Count == 0)
        {
            risks.Add("Критических пороговых рисков по текущим правилам не обнаружено; требуется предметная проверка.");
        }

        return new InsightsResponse(
            $"Обработано {rows.Count} строк. Факт {Money(totalActual)} при плане {Money(totalPlan)} " +
            $"({Percent(attainment)}), затраты {Money(totalCost)}. Самое слабое подразделение: " +
            $"{weakest.Department} ({Percent(weakest.AttainmentPercent)}).",
            anomalies,
            [
                $"Фактический показатель второй половины периода изменился на {Percent(trendPercent)} относительно первой.",
                $"Лучшее выполнение плана: {departmentMetrics.OrderByDescending(metric => metric.AttainmentPercent).First().Department}.",
                $"Максимальные затраты: {highestCost.Department} — {Money(highestCost.Cost)}."
            ],
            risks,
            [
                $"Почему {weakest.Department} выполняет только {Percent(weakest.AttainmentPercent)} плана?",
                "Какие из строк с задержкой требуют немедленного владельца и срока устранения?",
                $"Можно ли снизить затраты подразделения {highestCost.Department} без потери результата?",
                "Какие отклонения повторяются по одному продукту или категории?"
            ],
            evidence,
            departmentMetrics,
            new InsightAudit(
                "1.0.0",
                "deterministic-safe-preview",
                "Расчёты выполнены локально; внешней модели данные не передавались.",
                rows.Count,
                0,
                report.DetectedColumns,
                report.RedactedColumns));
    }

    private static string Money(decimal value) =>
        value.ToString("N0", CultureInfo.GetCultureInfo("ru-RU"));

    private static string Percent(decimal value) =>
        value.ToString("N1", CultureInfo.GetCultureInfo("ru-RU")) + "%";
}

