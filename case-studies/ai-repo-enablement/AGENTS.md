# Repository instructions for coding agents

## Objective

Make the smallest correct change that satisfies the assigned task. Preserve
existing behavior unless the task explicitly authorizes a behavior change.

## Repository map

- `src/` contains production projects.
- `tests/` contains automated tests.
- `docs/` contains user-facing and operational documentation.
- `.github/workflows/` contains CI definitions.

## Required workflow

1. Read the issue, this file and any nearer `AGENTS.md` before editing.
2. Inspect the affected code and tests; do not infer architecture from names.
3. State assumptions in the pull-request description.
4. Keep the diff bounded to the requested outcome.
5. Run restore, build and the relevant tests.
6. Report the exact commands and results.

## Validation

Use the repository's checked-in SDK and commands when present. The minimum
fallback for a .NET solution is:

```text
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Do not suppress warnings, delete failing tests or weaken assertions to make a
change pass.

## Architecture defaults

- Respect existing project and dependency boundaries.
- Prefer explicit cancellation and bounded timeouts on I/O.
- Avoid new global mutable state.
- Keep database calls observable and review generated SQL when changing EF Core
  queries.
- Treat public API, serialized contracts and database schemas as compatibility
  boundaries.

## Security and data

- Never print, copy, commit or invent credentials.
- Never use production data for tests.
- Do not modify deployment, identity, billing, payment, encryption or
  authorization behavior without explicit task scope and human review.
- Do not send repository content to an external service unless the owner has
  approved that service and data class.

## Pull requests

Include: problem, approach, risk, validation evidence and rollback note. A human
reviewer remains responsible for approval and merge.
