using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

// Original synthetic probe. Strictness is declared on the DTO, with no serializer options.
const string baseline = """{"Id":42,"Status":"Open"}""";
const string extra = """{"Id":42,"Status":"Open","Priority":"High"}""";
const string wrongType = """{"Id":"forty-two","Status":"Open"}""";

var observations = new[]
{
    Read<DefaultWorkItem>("default_baseline", baseline),
    Read<DefaultWorkItem>("default_extra", extra),
    Read<DefaultWorkItem>("default_wrong_type", wrongType),
    Read<StrictWorkItem>("attribute_baseline", baseline),
    Read<StrictWorkItem>("attribute_extra", extra),
    Read<StrictWorkItem>("attribute_wrong_type", wrongType)
};

// Expected results are literal assertions independent of the observed.json fixture.
var checks = new[]
{
    IsMapped(observations[0]),
    IsMapped(observations[1]),
    IsError(observations[2], "$.Id"),
    IsMapped(observations[3]),
    IsError(observations[4], "$.Priority"),
    IsError(observations[5], "$.Id")
};

Console.WriteLine(JsonSerializer.Serialize(new
{
    input_kind = "synthetic",
    configuration = "Default DTO versus [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] DTO; default serializer options for both",
    framework = RuntimeInformation.FrameworkDescription,
    runtime_version = Environment.Version.ToString(),
    target_framework = AppContext.TargetFrameworkName,
    cases = observations,
    check_count = checks.Length,
    all_checks_passed = checks.All(value => value)
}, new JsonSerializerOptions { WriteIndented = true }));
return checks.All(value => value) ? 0 : 1;

static Observation Read<T>(string caseId, string input) where T : class, IWorkItem
{
    try
    {
        var value = JsonSerializer.Deserialize<T>(input);
        return new(caseId, value is null ? "null" : "success", value?.Id, value?.Status, null, null);
    }
    catch (JsonException error)
    {
        return new(caseId, "JsonException", null, null, error.GetType().FullName, error.Path);
    }
}

static bool IsMapped(Observation value) =>
    value.Outcome == "success" && value.Id == 42 && value.Status == "Open"
    && value.ExceptionType is null && value.ExceptionPath is null;

static bool IsError(Observation value, string path) =>
    value.Outcome == "JsonException" && value.Id is null && value.Status is null
    && value.ExceptionType == "System.Text.Json.JsonException" && value.ExceptionPath == path;

public interface IWorkItem
{
    int Id { get; }
    string? Status { get; }
}

public sealed class DefaultWorkItem : IWorkItem
{
    public int Id { get; set; }
    public string? Status { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class StrictWorkItem : IWorkItem
{
    public int Id { get; set; }
    public string? Status { get; set; }
}

public sealed record Observation(
    string CaseId, string Outcome, int? Id, string? Status,
    string? ExceptionType, string? ExceptionPath);
