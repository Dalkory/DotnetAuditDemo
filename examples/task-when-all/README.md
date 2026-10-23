# Task.WhenAll: source and captured observations

An own synthetic teaching example from **dotnet-audit-studio**. It uses explicit `TaskCompletionSource` completions, with no client code, network calls, timed work or load generation.

The captured run in `observed.json` is dated **13 October 2026, 11:59:08 UTC**, using **.NET SDK 10.0.301 / runtime 10.0.9**. It covers three scenarios:

| Controlled inputs | Captured outcome |
| --- | --- |
| Two faults, one input still pending | The combined task remains incomplete until the last input completes; then it is faulted with both errors. |
| A fault and a cancellation | The combined task is faulted; cancellation is not added as a second fault. |
| Cancellations without faults | The combined task is canceled and its `Exception` is null. |

The saved verification passed **485 field, type and invariant checks across these three scenarios**. This is not 485 independent scenarios. No choice of the exception rethrown by `await`, error ordering, cancellation subtype, nested `AggregateException`/`Flatten`, retry behavior, recovery, production logging or production performance is guaranteed by this example.

## Inspect the evidence

- [Program.cs](Program.cs): controlled inputs and recorded snapshots.
- [expected.json](expected.json): separate literal expectations and limits.
- [observed.json](observed.json): the historical capture, not a fresh execution.
- [verify_observed.py](verify_observed.py): comparison of observations with expectations.
- [manifest.json](manifest.json): byte lengths and SHA256 hashes of the original bundle files.
- [README.txt](README.txt): original Russian explanation, commands and primary documentation links.

The nine original bundle files are retained unchanged from the public proof ZIP. This Markdown guide and the local `Directory.Build.props` are repository packaging additions and are not listed in that original manifest.

## Optional local reproduction

Run from **this directory** with SDK **10.0.301** and Python already installed. `global.json` disables SDK roll-forward; another SDK/runtime is not the recorded environment. The example has no package dependencies and its `NuGet.Config` clears package sources.

```text
dotnet restore TaskWhenAllProof.csproj --configfile NuGet.Config --ignore-failed-sources --nologo
dotnet build TaskWhenAllProof.csproj --no-restore --configuration Release --nologo -p:UseSharedCompilation=false
dotnet bin/Release/net10.0/TaskWhenAllProof.dll > observed-current.json
python verify_observed.py --observed observed-current.json --expected expected.json --receipt verification-current.json
```

To inspect only the saved capture, without executing C#:

```text
python verify_observed.py --observed observed.json --expected expected.json --receipt verification-current.json
```

This standalone .NET 10 example is outside the repository's main .NET 8 solution and CI. Its local empty `Directory.Build.props` prevents inheritance of the demo application's analyzer settings. The capture above comes from the original standalone run; publishing these files does not constitute a new fixture run or an integration-test result.

## Related note and paid review

Read the [Russian Task.WhenAll note](https://dotnet-audit-studio.dtaatdaa.chatgpt.site/ru/notes/task-when-all-exceptions) or watch the [existing explanation](https://www.youtube.com/watch?v=7o-8zznYwz0).

For a [paid review of a bounded code sample](https://dotnet-audit-studio.dtaatdaa.chatgpt.site/en?intent=paid-review#request), scope, permitted materials, acceptance, price, timing and private-code AI permission are agreed before work begins.

The repository's [MIT license](../../LICENSE) applies.
