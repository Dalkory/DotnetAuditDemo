using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

// Original synthetic fixture, dotnet-audit-studio. No HTTP or customer input.
const string baseline = """{"Id":42,"Status":"Open"}""";
const string extended = """{"Id":42,"Status":"Open","Priority":"High"}""";
const string wrongType = """{"Id":"forty-two","Status":"Open"}""";

var cases = new[]
{
    Observe("default_baseline", "default", baseline, null),
    Observe("default_extra", "default", extended, null),
    Observe("skip_extra", "Skip", extended, new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip }),
    Observe("strict_baseline", "Disallow", baseline, new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }),
    Observe("strict_extra", "Disallow", extended, new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }),
    Observe("default_wrong_type", "default", wrongType, null),
    Observe("strict_wrong_type", "Disallow", wrongType, new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
};

Console.WriteLine(JsonSerializer.Serialize(new
{
    brand = "dotnet-audit-studio",
    input_kind = "synthetic",
    scope = "One DTO; literal JSON; synchronous in-memory System.Text.Json deserialization; no HTTP/API client/custom converter",
    framework = RuntimeInformation.FrameworkDescription,
    environment_version = Environment.Version.ToString(),
    system_text_json_assembly_version = typeof(JsonSerializer).Assembly.GetName().Version?.ToString(),
    target_framework = AppContext.TargetFrameworkName,
    cases
}, new JsonSerializerOptions { WriteIndented = true }));

static Observation Observe(string id, string mode, string json, JsonSerializerOptions? options)
{
    try
    {
        var dto = JsonSerializer.Deserialize<WorkItemDto>(json, options);
        return new(id, mode, json, dto is null ? "null" : "success", dto?.Id, dto?.Status, null, null);
    }
    catch (JsonException ex)
    {
        // The path is relevant evidence; avoid leaking local stack traces/paths.
        return new(id, mode, json, "JsonException", null, null, ex.GetType().FullName, ex.Path);
    }
}

public sealed class WorkItemDto
{
    public int Id { get; set; }
    public string? Status { get; set; }
}

public sealed record Observation(
    string CaseId,
    string Mode,
    string Input,
    string Outcome,
    int? Id,
    string? Status,
    string? ExceptionType,
    string? ExceptionPath);
