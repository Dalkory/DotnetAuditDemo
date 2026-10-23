# Unknown JSON properties: source and saved controls

Own synthetic **dotnet-audit-studio** programs, with literal JSON and in-memory `System.Text.Json` deserialization. The original 12 October 2026 run used **.NET SDK 10.0.301 / runtime 10.0.9**.

- [fixture/Program.cs](fixture/Program.cs), [saved observation](fixture/observed.json), [literal checker](fixture/verify_expected.py) and [saved check](fixture/expectation-check.json): seven cases covering baseline input, an extra property, explicit Skip/Disallow, and a wrong type in a mapped property.
- [attribute-probe/Program.cs](attribute-probe/Program.cs) and [saved observation](attribute-probe/observed-attribute.json): a separate six-case probe comparing a default DTO with a DTO declaring `JsonUnmappedMemberHandling.Disallow`.

In those captures, the extra `Priority` property is accepted by default/Skip and rejected by Disallow at `$.Priority`. A wrong type for mapped `Id` is rejected at `$.Id`; it is a separate control. The saved checker reports PASS for the seven original cases; the attribute probe records six passing checks. These remain dated observations, not new execution results.

## Optional reproduction

The two programs are kept in separate projects. From this directory, with SDK 10.0.301 already installed:

```text
dotnet restore fixture/JsonUnmappedDemo.csproj --configfile NuGet.Config --ignore-failed-sources --nologo
dotnet build fixture/JsonUnmappedDemo.csproj --configuration Release --no-restore --nologo
dotnet fixture/bin/Release/net10.0/JsonUnmappedDemo.dll
dotnet restore attribute-probe/IndependentProbe.csproj --configfile NuGet.Config --ignore-failed-sources --nologo
dotnet build attribute-probe/IndependentProbe.csproj --configuration Release --no-restore --nologo
dotnet attribute-probe/bin/Release/net10.0/IndependentProbe.dll
```

To check only the saved original JSON, with Python installed and without running C#:

```text
python fixture/verify_expected.py fixture/observed.json
```

`global.json` disables SDK roll-forward. The original projects have no package references and `NuGet.Config` clears package sources. The local empty `Directory.Build.props` stops inheritance of the main demo application's analyzer settings. These standalone projects are outside the main .NET 8 solution and CI; no new repository build or test result is claimed here.

This exercise does not test HTTP clients, ASP.NET model binding, custom converters, enums, a cross-version matrix, performance or production compatibility.

Read the [English note](https://dotnet-audit-studio.dtaatdaa.chatgpt.site/en/notes/json-unknown-fields) or return to the [four-note library](../).
