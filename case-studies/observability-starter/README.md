# Observability Starter for .NET — demonstration

This self-contained ASP.NET Core sample demonstrates the concrete output of a
focused observability sprint for one service:

- OpenTelemetry resource and service identity;
- ASP.NET Core, `HttpClient`, and runtime instrumentation;
- OTLP trace and metric export;
- `/health` and `/alive` endpoints;
- one custom activity, counter, and latency histogram;
- an incident runbook tied to a real request path.

The sample exports through the standard OTLP environment variables. For local
testing, point `OTEL_EXPORTER_OTLP_ENDPOINT` at an OpenTelemetry Collector,
Aspire dashboard, or another OTLP-compatible backend.

```powershell
$env:OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
dotnet run --project src/ObservabilityStarter.Api
```

Then call:

```text
GET /health
GET /alive
GET /checkout/42
```

The custom telemetry intentionally records only a numeric demo order identifier
and a low-cardinality result. A real engagement defines a data-classification
policy before adding business attributes.

See [`INCIDENT_RUNBOOK.md`](INCIDENT_RUNBOOK.md) for the operational use of the
signals.
