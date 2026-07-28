# Incident runbook: slow checkout request

## Trigger

Investigate when checkout latency or error rate deviates from the agreed
baseline.

## Triage

1. Check `/alive` to distinguish process availability from dependency health.
2. Check `/health` for the current health-check result.
3. Open a slow server trace and locate the `checkout.process` activity.
4. Compare server duration with child dependency spans.
5. Check `checkout.duration.ms`, ASP.NET request duration, runtime CPU, GC, and thread-pool signals.
6. Correlate the trace with structured logs using trace ID; do not search by customer PII.
7. Record the suspected component and evidence before changing timeout, retry, or concurrency settings.

## Decision guide

- Long server span with no long dependency: inspect application work, locks, and thread-pool pressure.
- Long dependency span: inspect provider latency, timeout budget, retry policy, and connection pool.
- Normal trace but elevated queue time: inspect concurrency limits and upstream backpressure.
- Health failure plus request errors: prioritize containment and dependency recovery.

## Verification after remediation

- Repeat the same request profile.
- Compare p50/p95/p99 latency and error rate.
- Confirm trace completeness and bounded telemetry cardinality.
- Confirm no secrets or personal data were added to telemetry.
