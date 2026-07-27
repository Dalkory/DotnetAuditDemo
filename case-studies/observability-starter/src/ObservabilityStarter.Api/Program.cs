using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "ObservabilityStarter.Api";
var activitySource = new ActivitySource(serviceName);
var meter = new Meter(serviceName);
var checkoutCounter = meter.CreateCounter<long>("checkout.requests");
var checkoutDuration = meter.CreateHistogram<double>("checkout.duration.ms");

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(serviceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/alive", () => Results.Ok(new { status = "alive" }));

app.MapGet("/checkout/{orderId:int}", async (int orderId, CancellationToken cancellationToken) =>
{
    var stopwatch = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("checkout.process");
    activity?.SetTag("order.id", orderId);

    await Task.Delay(TimeSpan.FromMilliseconds(120 + orderId % 4 * 80), cancellationToken);

    checkoutCounter.Add(1, new KeyValuePair<string, object?>("result", "accepted"));
    checkoutDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
    activity?.SetTag("checkout.result", "accepted");

    return Results.Ok(new { orderId, status = "accepted" });
});

app.Run();
