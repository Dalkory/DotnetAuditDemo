using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

// Synthetic teaching fixture: explicit completions, no timed work or parallel clients.
var scenarios = new[]
{
    await TwoFaultsAndPendingSuccess(),
    await FaultAndCancellation(),
    await CancellationsOnly()
};
var sdkVersion = Assembly.GetExecutingAssembly()
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .Single(item => item.Key == "BuildSdkVersion").Value;
var proof = new ProofDocument(
    1,
    "demo:task-when-all-explicit-completion",
    DateTimeOffset.UtcNow,
    new RuntimeView(Environment.Version.ToString(), RuntimeInformation.FrameworkDescription, sdkVersion),
    scenarios);
Console.WriteLine(JsonSerializer.Serialize(proof, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    WriteIndented = true
}));

static TaskCompletionSource<int> ControlledTask() =>
    new(TaskCreationOptions.RunContinuationsAsynchronously);

static async Task<ScenarioView> TwoFaultsAndPendingSuccess()
{
    var first = ControlledTask();
    var second = ControlledTask();
    var third = ControlledTask();
    var inputs = new (string Id, Task Task)[]
    {
        ("demo:fault-a", first.Task),
        ("demo:fault-b", second.Task),
        ("demo:pending-success", third.Task)
    };
    Task combined = Task.WhenAll(inputs.Select(item => item.Task).ToArray());
    first.SetException(new InvalidOperationException("demo:first-fault"));
    second.SetException(new ArgumentException("demo:second-fault"));
    var before = Snapshot(combined, inputs);
    third.SetResult(3);
    var awaited = await CaptureAwait(combined);
    return new ScenarioView("demo:two-faults-third-pending", before, Snapshot(combined, inputs), awaited);
}

static async Task<ScenarioView> FaultAndCancellation()
{
    var fault = ControlledTask();
    var canceled = ControlledTask();
    var inputs = new (string Id, Task Task)[]
    {
        ("demo:fault", fault.Task),
        ("demo:canceled", canceled.Task)
    };
    Task combined = Task.WhenAll(inputs.Select(item => item.Task).ToArray());
    fault.SetException(new InvalidOperationException("demo:mixed-fault"));
    var before = Snapshot(combined, inputs);
    canceled.SetCanceled();
    var awaited = await CaptureAwait(combined);
    return new ScenarioView("demo:fault-plus-canceled", before, Snapshot(combined, inputs), awaited);
}

static async Task<ScenarioView> CancellationsOnly()
{
    var first = ControlledTask();
    var second = ControlledTask();
    var inputs = new (string Id, Task Task)[]
    {
        ("demo:canceled-a", first.Task),
        ("demo:canceled-b", second.Task)
    };
    Task combined = Task.WhenAll(inputs.Select(item => item.Task).ToArray());
    first.SetCanceled();
    var before = Snapshot(combined, inputs);
    second.SetCanceled();
    var awaited = await CaptureAwait(combined);
    return new ScenarioView("demo:only-canceled", before, Snapshot(combined, inputs), awaited);
}

static CompletionSnapshot Snapshot(Task combined, (string Id, Task Task)[] inputs) =>
    new(Inspect("demo:when-all", combined), inputs.Select(item => Inspect(item.Id, item.Task)).ToArray());

static TaskView Inspect(string id, Task task)
{
    // Sorting presents an error set; neither input order nor await's choice is a contract here.
    var faults = task.Exception?.InnerExceptions
        .Select(error => new ErrorView(error.GetType().FullName!, error.Message))
        .OrderBy(error => error.Type, StringComparer.Ordinal)
        .ThenBy(error => error.Message, StringComparer.Ordinal)
        .ToArray() ?? [];
    return new TaskView(id, task.Status.ToString(), task.IsCompleted, task.IsFaulted,
        task.IsCanceled, task.Exception is null, faults.Length, faults);
}

static async Task<AwaitObservation> CaptureAwait(Task combined)
{
    try
    {
        await combined;
        return new AwaitObservation(true, null);
    }
    catch (Exception error)
    {
        return new AwaitObservation(false,
            new CaughtErrorView(error.GetType().FullName!, error.Message, error is OperationCanceledException));
    }
}

sealed record ProofDocument(int SchemaVersion, string ProofKind, DateTimeOffset ObservedAtUtc,
    RuntimeView Runtime, ScenarioView[] Scenarios);
sealed record RuntimeView(string RuntimeVersion, string FrameworkDescription, string? SdkVersion);
sealed record ScenarioView(string Id, CompletionSnapshot BeforeFinalCompletion,
    CompletionSnapshot AfterFinalCompletion, AwaitObservation Await);
sealed record CompletionSnapshot(TaskView Combined, TaskView[] Inputs);
sealed record TaskView(string Id, string Status, bool IsCompleted, bool IsFaulted, bool IsCanceled,
    bool ExceptionIsNull, int FaultCount, ErrorView[] Faults);
sealed record ErrorView(string Type, string Message);
sealed record AwaitObservation(bool ReturnedNormally, CaughtErrorView? Caught);
sealed record CaughtErrorView(string Type, string Message, bool IsOperationCanceledException);
