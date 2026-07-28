using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace AiInsights.Web;

public static class SampleReport
{
    private static readonly string[] Departments = ["Sales", "Operations", "Support", "Delivery"];
    private static readonly string[] Products = ["Core", "Enterprise", "Reports", "Integrations"];

    public static byte[] CreateXlsx(int rowCount = 2_000)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Report");
        var headers = new[] { "Date", "Department", "Product", "Plan", "Actual", "Cost", "DelayDays", "ClientEmail" };
        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
        }

        var random = new Random(20260728);
        for (var index = 0; index < rowCount; index++)
        {
            var row = index + 2;
            var date = new DateTime(2026, 1, 1).AddDays(index % 180);
            var department = Departments[index % Departments.Length];
            var product = Products[(index / Departments.Length) % Products.Length];
            var plan = 12_000m + (index % 17) * 500m;
            var departmentFactor = department switch
            {
                "Support" => 0.76m,
                "Operations" => 0.92m,
                "Delivery" => 1.03m,
                _ => 1.08m
            };
            var noise = (decimal)(random.NextDouble() * 0.22 - 0.11);
            var actual = decimal.Round(plan * (departmentFactor + noise), 2);
            var cost = decimal.Round(actual * (0.42m + (index % 9) / 100m), 2);
            var delay = department == "Support" && index % 11 == 0 ? 12 : random.Next(0, 6);

            sheet.Cell(row, 1).Value = date;
            sheet.Cell(row, 2).Value = department;
            sheet.Cell(row, 3).Value = product;
            sheet.Cell(row, 4).Value = plan;
            sheet.Cell(row, 5).Value = actual;
            sheet.Cell(row, 6).Value = cost;
            sheet.Cell(row, 7).Value = delay;
            sheet.Cell(row, 8).Value = $"client-{index + 1}@example.invalid";
        }

        sheet.Column(1).Style.DateFormat.Format = "yyyy-MM-dd";
        sheet.Columns().AdjustToContents();
        sheet.SheetView.FreezeRows(1);
        sheet.RangeUsed()!.CreateTable("SyntheticReport");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] CreateCsv(int rowCount = 2_000)
    {
        var builder = new StringBuilder("Date,Department,Product,Plan,Actual,Cost,DelayDays,ClientEmail\n");
        var random = new Random(20260728);
        for (var index = 0; index < rowCount; index++)
        {
            var date = new DateOnly(2026, 1, 1).AddDays(index % 180);
            var department = Departments[index % Departments.Length];
            var product = Products[(index / Departments.Length) % Products.Length];
            var plan = 12_000m + (index % 17) * 500m;
            var factor = department == "Support" ? 0.76m : department == "Operations" ? 0.92m : department == "Delivery" ? 1.03m : 1.08m;
            var actual = decimal.Round(plan * (factor + (decimal)(random.NextDouble() * 0.22 - 0.11)), 2);
            var cost = decimal.Round(actual * (0.42m + (index % 9) / 100m), 2);
            var delay = department == "Support" && index % 11 == 0 ? 12 : random.Next(0, 6);
            builder.AppendLine(string.Join(",",
                date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                department,
                product,
                plan.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture),
                cost.ToString(CultureInfo.InvariantCulture),
                delay.ToString(CultureInfo.InvariantCulture),
                $"client-{index + 1}@example.invalid"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}

