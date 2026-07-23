# Prioritized implementation plan

This plan turns the 15 audit findings into release-sized work. Estimates are
relative and should be recalibrated after repository access, production
constraints and ownership are confirmed.

## Phase 0 — Contain immediate risk

**Target:** same day

| Order | Findings | Change | Exit condition |
|---|---|---|---|
| 1 | F-003 | Rotate real credentials, remove tracked values and load them from approved secret storage | No usable secret in source or history; application starts from injected config |
| 2 | F-002 | Protect `/api/admin/users` with an authorization policy | Anonymous request is `401`; unauthorized identity is `403` |
| 3 | F-011 | Remove credentials/PII from logs and add redaction guidance | Automated test proves password is not emitted |
| 4 | F-001 | Restore scoped `DbContext`; create scopes in hosted work | Concurrent request/worker test passes without shared context |

Rollback must be defined before rotating production credentials or changing an
authorization policy.

## Phase 1 — Stabilize request execution

**Target:** 1–3 days

| Order | Findings | Change | Exit condition |
|---|---|---|---|
| 1 | F-007 | Replace `.Result` with `await` throughout request and worker paths | No sync-over-async remains in application code |
| 2 | F-008 | Add explicit HTTP timeout and bounded resilience policy | Dependency stall fails within the agreed budget |
| 3 | F-006 | Propagate request/shutdown cancellation tokens | Cancelled request stops EF/HTTP work |
| 4 | F-010 | Add centralized `ProblemDetails` exception mapping | Expected failures have stable status/type; unexpected failures retain correlation IDs |
| 5 | F-015 | Add liveness/readiness checks and deployment probes | Database outage removes instance from readiness |

## Phase 2 — Protect data quality and query performance

**Target:** 2–4 days

| Order | Findings | Change | Exit condition |
|---|---|---|---|
| 1 | F-009 | Validate order command and return structured `400` errors | Invalid/edge-case tests pass |
| 2 | F-004 | Add capped cursor or page pagination and no-tracking projection | Response size and query time remain bounded |
| 3 | F-005 | Replace per-order customer lookup with one server-side projection | Constant SQL-command count per page |
| 4 | F-012 | Add the external-reference index through a reviewed migration | Representative `EXPLAIN ANALYZE` uses the index |

## Phase 3 — Improve change safety and AI readiness

**Target:** 3–7 days

| Order | Findings | Change | Exit condition |
|---|---|---|---|
| 1 | F-013 | Introduce Application use cases and persistence interfaces | API no longer references EF Core context |
| 2 | F-014 | Add security, validation, failure, cancellation and concurrency tests | Risk-based test matrix runs in CI |
| 3 | F-013, F-014 | Add repository guidance and deterministic developer workflow | Human or coding agent can build/test/change one use case from README alone |

## Demonstration pull request

The portfolio remediation PR intentionally addresses five findings:

1. F-001 — scoped `DbContext`
2. F-002 — authenticated administrative endpoint
3. F-003 — secrets removed from tracked configuration
4. F-004 — bounded pagination
5. F-005 — single projected query instead of N+1

The remaining findings stay visible so the repository continues to function as a
realistic audit case rather than pretending one PR completes the full program.

## Delivery and verification

For every implementation batch:

1. record the finding IDs in the PR;
2. add or update a regression test;
3. run restore, Release build and tests;
4. note configuration/migration/rollback steps;
5. verify logs contain no private client code or credentials;
6. update finding status and residual risk.
