# Legacy .NET Modernization — demonstration assessment

This is a synthetic, non-client case that demonstrates the deliverables of a
fixed-scope modernization assessment. The input under `sample/` intentionally
resembles a small .NET Framework 4.7.2 ASP.NET MVC / EF6 application. It is not
presented as production code or as a completed migration.

Start with:

1. [`ASSESSMENT.md`](ASSESSMENT.md) — executive summary and current-state inventory.
2. [`BLOCKERS.md`](BLOCKERS.md) — evidence-backed compatibility and delivery blockers.
3. [`MIGRATION_PLAN.md`](MIGRATION_PLAN.md) — ordered pilot plan and acceptance gates.

The assessment separates planning from execution. A real engagement would also
inspect build logs, package feeds, runtime configuration, deployment topology,
database behavior, authentication flows, and representative tests.
