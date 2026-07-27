# Ordered migration plan

## Phase 0 — evidence

- Reproduce the current build with documented prerequisites.
- Export the package graph and identify unsupported dependencies.
- Record representative requests, responses, SQL, and authentication behavior.
- Add tests for the read-only orders path and one write path.

Exit gate: the current behavior can be verified without manual guesswork.

## Phase 1 — target foundation

- Create an SDK-style ASP.NET Core host on a supported LTS .NET release.
- Add configuration providers, secret handling, structured logging, and health endpoints.
- Add CI build, tests, package audit, and warnings policy.

Exit gate: the empty target application builds, tests, and deploys in the chosen environment.

## Phase 2 — first vertical slice

- Port the read-only orders endpoint.
- Keep the existing database schema.
- Compare result shape, SQL, latency, and failure behavior.
- Route a controlled test workload to the new endpoint.

Exit gate: functional parity is demonstrated and rollback is documented.

## Phase 3 — writes and data strategy

- Port the create-order path with validation and cancellation.
- Decide EF6 bridge vs EF Core based on mappings, provider support, and query evidence.
- Add transaction and concurrency tests.

Exit gate: write semantics and failure behavior are covered by automated tests.

## Phase 4 — authentication and cutover

- Implement the selected modern authentication scheme.
- Map roles and authorization policies.
- Execute staged traffic migration and monitor errors, latency, and database load.

Exit gate: operational acceptance criteria pass and the legacy path can be retired safely.

## Explicit exclusions

UI redesign, cloud re-platforming, database redesign, and organization-wide
identity changes are not assumed to be part of the first pilot.
