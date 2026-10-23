# Four reproducible learning notes

Source code, fixed synthetic inputs, historical observations and explicit limits from **dotnet-audit-studio**. These are teaching examples, not client cases or production benchmarks.

| Existing topic | Source and recorded evidence | Recorded environment |
| --- | --- | --- |
| Lost updates and revision checks | [Lost update](lost-update/) | Python / SQLite 3.45.3 |
| Unknown JSON properties | [System.Text.Json](json-unknown-fields/) | .NET SDK 10.0.301 / runtime 10.0.9 |
| A unique index with existing duplicates | [Unique index](unique-index/) | Python / SQLite; exact versions in the capture |
| Task.WhenAll exceptions and cancellation | [Task.WhenAll](task-when-all/) | .NET SDK 10.0.301 / runtime 10.0.9 |

Each guide separates what the original run observed from what it did not test. Publishing this library does not add new executions, scenarios, production evidence, customers or sales results. Python and .NET runtime requirements differ; there is no combined test runner. The standalone .NET 10 examples are outside the main .NET 8 solution and CI.

[source-manifest.json](source-manifest.json) records byte hashes for the three original bundles added alongside Task.WhenAll. Task.WhenAll has its [original manifest](task-when-all/manifest.json). The repository's [MIT license](../LICENSE) applies.

For a [paid learning review](https://dotnet-audit-studio.dtauskanov3.chatgpt.site/en?intent=paid-review#request) or a [bounded studio result](https://dotnet-audit-studio.dtauskanov3.chatgpt.site/en#request), scope, permitted materials, acceptance, price, timing and private-code AI permission are agreed before work begins.
