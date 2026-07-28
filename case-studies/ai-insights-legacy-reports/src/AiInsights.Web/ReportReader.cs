using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace AiInsights.Web;

public sealed class ReportReader
{
    private static readonly Dictionary<string, string[]> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Date"] = ["date", "period", "day", "дата", "период"],
        ["Department"] = ["department", "team", "unit", "отдел", "подразделение"],
        ["Product"] = ["product", "service", "category", "продукт", "услуга"],
        ["Plan"] = ["plan", "target", "budget", "план"],
        ["Actual"] = ["actual", "fact", "revenue", "факт", "выручка"],
        ["Cost"] = ["cost", "expense", "spend", "затраты", "расход"],
        ["DelayDays"] = ["delaydays", "delay", "late", "просрочка", "днипросрочки"]
    };

    private static readonly string[] SensitiveTokens =
    [
        "email", "e-mail", "phone", "mobile", "name", "fullname", "passport",
        "card", "account", "client", "customer", "почта", "телефон", "фио",
        "паспорт", "карта", "счёт", "счет", "клиент"
    ];

    public async Task<ParsedReport> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        await using var stream = file.OpenReadStream();

        return extension.ToLowerInvariant() switch
        {
            ".csv" => await ReadCsvAsync(stream, cancellationToken),
            ".xlsx" => ReadXlsx(stream),
            _ => throw new InvalidDataException("Поддерживаются только CSV и XLSX.")
        };
    }

    private static async Task<ParsedReport> ReadCsvAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken)
            ?? throw new InvalidDataException("Файл пуст.");
        var delimiter = DetectDelimiter(headerLine);
        var headers = ParseCsvLine(headerLine, delimiter);
        var mapping = BuildMapping(headers);
        var rows = new List<ReportRow>();
        var sourceRow = 1;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            sourceRow++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line, delimiter);
            rows.Add(ParseRow(values, mapping, sourceRow));
        }

        return BuildResult(rows, headers);
    }

    private static ParsedReport ReadXlsx(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var used = worksheet.RangeUsed() ?? throw new InvalidDataException("Лист Excel пуст.");
        var headers = used.FirstRow().Cells().Select(cell => cell.GetString().Trim()).ToArray();
        var mapping = BuildMapping(headers);
        var rows = new List<ReportRow>();

        foreach (var row in used.RowsUsed().Skip(1))
        {
            var values = row.Cells(1, headers.Length)
                .Select(cell => cell.GetFormattedString())
                .ToArray();
            rows.Add(ParseRow(values, mapping, row.RowNumber()));
        }

        return BuildResult(rows, headers);
    }

    private static ParsedReport BuildResult(IReadOnlyList<ReportRow> rows, IReadOnlyList<string> headers)
    {
        if (rows.Count == 0)
        {
            throw new InvalidDataException("В файле нет строк данных.");
        }

        var redacted = headers
            .Where(header => SensitiveTokens.Any(token =>
                Normalize(header).Contains(Normalize(token), StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ParsedReport(rows, headers, redacted);
    }

    private static Dictionary<string, int> BuildMapping(IReadOnlyList<string> headers)
    {
        var normalized = headers
            .Select((header, index) => (Header: Normalize(header), Index: index))
            .ToArray();
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in Aliases)
        {
            var match = normalized.FirstOrDefault(item =>
                field.Value.Select(Normalize).Contains(item.Header, StringComparer.OrdinalIgnoreCase));
            if (match.Header is null)
            {
                throw new InvalidDataException($"Не найдена обязательная колонка: {field.Key}.");
            }

            mapping[field.Key] = match.Index;
        }

        return mapping;
    }

    private static ReportRow ParseRow(IReadOnlyList<string> values, IReadOnlyDictionary<string, int> mapping, int sourceRow)
    {
        string At(string field)
        {
            var index = mapping[field];
            return index < values.Count ? values[index].Trim() : string.Empty;
        }

        if (!TryParseDate(At("Date"), out var date) ||
            !TryParseDecimal(At("Plan"), out var plan) ||
            !TryParseDecimal(At("Actual"), out var actual) ||
            !TryParseDecimal(At("Cost"), out var cost) ||
            !int.TryParse(At("DelayDays"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var delay))
        {
            throw new InvalidDataException($"Строка {sourceRow}: неверный формат даты или числового значения.");
        }

        return new ReportRow(sourceRow, date, At("Department"), At("Product"), plan, actual, cost, delay);
    }

    private static bool TryParseDate(string value, out DateOnly date) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date) ||
        DateOnly.TryParse(value, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AllowWhiteSpaces, out date);

    private static bool TryParseDecimal(string value, out decimal number)
    {
        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out number) ||
               decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU"), out number);
    }

    private static char DetectDelimiter(string line)
    {
        var candidates = new[] { ',', ';', '\t' };
        return candidates.OrderByDescending(candidate => line.Count(character => character == candidate)).First();
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == delimiter && !quoted)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

public sealed record ParsedReport(
    IReadOnlyList<ReportRow> Rows,
    IReadOnlyList<string> DetectedColumns,
    IReadOnlyList<string> RedactedColumns);

