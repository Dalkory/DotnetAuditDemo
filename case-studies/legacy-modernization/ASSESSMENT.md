# Modernization assessment

## Executive summary

The sample is a small ASP.NET MVC 5 application targeting .NET Framework 4.7.2
with EF6, `packages.config`, `System.Web`, Forms Authentication, IIS-specific
hosting, and configuration-bound infrastructure. A direct target-framework
edit is not a safe migration.

Recommended target: an ASP.NET Core application on a currently supported LTS
.NET release, with SDK-style projects, package references, explicit dependency
injection, externalized configuration, and characterization tests around the
orders flow.

The lowest-risk first pilot is the read-only orders endpoint. It has a narrow
dependency surface and can prove the target hosting, configuration, database,
and delivery path without moving authentication and writes at the same time.

## Inventory

| Area | Current state | Modernization implication |
|---|---|---|
| Runtime | .NET Framework 4.7.2 | Move to supported modern .NET |
| Web | ASP.NET MVC 5 / `System.Web` | Port to ASP.NET Core controllers or minimal APIs |
| Data | EF6 / `System.Data.Entity` | Decide EF6-on-modern-.NET bridge vs EF Core migration |
| Project | Old MSBuild XML | Convert to SDK-style project |
| Packages | `packages.config` | Convert to `PackageReference` and refresh dependencies |
| Hosting | IIS / `Web.config` | Define Kestrel + reverse proxy or container target |
| Authentication | Forms Authentication | Replace with an explicit modern auth scheme |
| Configuration | `Web.config` and connection strings | Move to configuration providers and secret storage |
| Tests | None in sample | Add characterization and contract tests before behavior changes |

## Decision

Proceed with an incremental migration. Do not combine framework migration,
authentication replacement, EF Core conversion, UI redesign, and infrastructure
changes in one release.

## Proposed pilot

1. Create an SDK-style ASP.NET Core host.
2. Add configuration and database connectivity without copying secrets.
3. Characterize the existing orders query and response.
4. Port the read-only endpoint.
5. Compare behavior and SQL.
6. Add CI build and tests.
7. Decide whether the next slice is writes, authentication, or data access.

## Estimate bands

These are demonstration bands, not a client quote:

- assessment and validated plan: 1–3 working days;
- first read-only pilot: 2–5 working days;
- authentication, writes, and deployment: separately estimated after the pilot.
