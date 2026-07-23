# .NET Backend & AI Readiness Audit

**Project:** DotnetAuditDemo

**Review type:** code-focused demonstration audit

**Scope:** ASP.NET Core API, application/domain/infrastructure projects, EF Core,
PostgreSQL configuration, authentication, background processing, tests, Docker
and CI

**Reviewed by:** Daniil Taushkanov

**Contact:** taushkanovdaniil@gmail.com · @DanilDotNet

## 1. Executive summary

The service has a recognizable layered structure and can be built, tested and
started locally. However, the review found 15 deliberate defects. Four are
immediate security or concurrency risks, six can cause degraded reliability or
performance under load, and five reduce change safety and operability.

The recommended first release gate is to remove committed secrets, protect the
administrative route, correct the `DbContext` lifetime and stop logging
credentials. Performance work should then eliminate the unbounded N+1 order
query and add the missing index. The remaining work can follow as a measured
hardening backlog.

### Priority summary

| ID | Priority | Severity | Category | Finding | Estimate |
|---|---|---|---|---|---|
| F-001 | P0 | High | Reliability | `DbContext` registered as singleton | S |
| F-002 | P0 | Critical | Security | Administrative endpoint is unauthenticated | S |
| F-003 | P0 | Critical | Security | Secrets committed in configuration | S |
| F-011 | P0 | Critical | Security | Credentials and PII written to logs | S |
| F-004 | P1 | High | Performance | Orders endpoint materializes the full table | M |
| F-005 | P1 | High | Database | N+1 customer queries | M |
| F-006 | P1 | Medium | Reliability | Cancellation is not propagated | M |
| F-007 | P1 | High | Reliability | Sync-over-async in request and worker paths | S |
| F-008 | P1 | High | Reliability | Outbound HTTP client has no timeout | S |
| F-009 | P1 | High | Data integrity | Order input is not validated | M |
| F-012 | P1 | Medium | Database | Missing index for external-reference lookup | S |
| F-015 | P1 | High | Operations | No dependency-aware health checks | M |
| F-010 | P2 | Medium | Maintainability | Broad exception catch masks failure types | M |
| F-013 | P2 | Medium | Architecture | Layer boundaries are bypassed | L |
| F-014 | P2 | Medium | Testing | Tests cover happy paths only | M |

Effort uses relative sizes: **S** ≤ 0.5 day, **M** 1–2 days, **L** 3–5 days.

## 2. Scope and approach

Reviewed artifacts:

- `src/DotnetAuditDemo.Api`
- `src/DotnetAuditDemo.Application`
- `src/DotnetAuditDemo.Domain`
- `src/DotnetAuditDemo.Infrastructure`
- `tests/DotnetAuditDemo.Tests`
- container, configuration and CI files

The review used code tracing, configuration inspection, API threat modeling,
query-shape analysis and local build/test execution. Production telemetry,
cloud resources, deployment credentials and a live database were out of scope.

## 3. Detailed findings

### F-001 — Singleton `DbContext`

- **Severity / priority:** High / P0
- **Location:** `Program.cs`, `AddDbContext<DemoDbContext>`
- **Why it matters:** EF Core contexts are not thread-safe. A singleton shares
  tracking state and database operations across concurrent requests and the
  background worker, risking cross-request data leakage and
  `A second operation was started on this context` failures.
- **Reproduce:** Send concurrent requests to `/api/orders` while the background
  worker queries orders; inspect exceptions and growing tracked-entity count.
- **Recommendation:** Restore the scoped lifetime and create an explicit service
  scope inside the hosted worker.
- **Effort:** S

### F-002 — Unprotected administrative endpoint

- **Severity / priority:** Critical / P0
- **Location:** `AdminController.GetUsers`
- **Why it matters:** Any caller can enumerate customer identifiers, names and
  email addresses even though authentication is configured elsewhere.
- **Reproduce:** `curl http://localhost:8080/api/admin/users` without an
  `Authorization` header returns `200`.
- **Recommendation:** Apply an authorization policy, test `401/403` behavior and
  grant the policy only to a trusted administrative role.
- **Effort:** S

### F-003 — Secrets committed to source control

- **Severity / priority:** Critical / P0
- **Location:** `appsettings.json`, `Jwt:SigningKey` and `Payments:ApiKey`
- **Why it matters:** Repository readers can forge tokens or call downstream
  services if demo values are ever replaced with real credentials. Git history
  keeps deleted values.
- **Reproduce:** Search tracked files for `SigningKey`, `ApiKey` or known secret
  prefixes.
- **Recommendation:** Rotate any real value, purge it from history when
  necessary, load secrets from a secret manager/environment and add automated
  secret scanning.
- **Effort:** S

### F-004 — Unbounded order materialization

- **Severity / priority:** High / P1
- **Location:** `OrdersController.GetAll`
- **Why it matters:** `ToListAsync()` reads every order and allocates the entire
  response in memory. Latency and memory grow with table size and can exhaust
  the process.
- **Reproduce:** Seed 100,000 orders, call `GET /api/orders`, then record query
  duration, response size and process memory.
- **Recommendation:** Require bounded pagination, cap page size, use
  `AsNoTracking()` and project only response fields.
- **Effort:** M

### F-005 — N+1 customer lookup

- **Severity / priority:** High / P1
- **Location:** `OrdersController.GetAll`, loop over `dbContext.Customers`
- **Why it matters:** A page with N orders produces one order query plus N
  customer queries, multiplying database round trips.
- **Reproduce:** Enable EF Core command logging and count SQL commands for a
  single `/api/orders` request.
- **Recommendation:** Project orders and customer fields in one server-side
  query or use a justified eager load.
- **Effort:** M

### F-006 — Cancellation is not propagated

- **Severity / priority:** Medium / P1
- **Location:** order controller actions, seeding and exchange-rate client
- **Why it matters:** Work continues after clients disconnect or deployments
  begin shutting down, consuming database and HTTP capacity.
- **Reproduce:** Start a slow order request, cancel the client connection and
  observe the database command continuing.
- **Recommendation:** Accept `CancellationToken` at request boundaries and pass
  it through EF Core, HTTP and persistence calls.
- **Effort:** M

### F-007 — Sync-over-async

- **Severity / priority:** High / P1
- **Location:** `OrdersController.Create`; `InvoiceReminderWorker.ExecuteAsync`
- **Why it matters:** `.Result` blocks worker threads and can cause thread-pool
  starvation, poor throughput and shutdown delays.
- **Reproduce:** Make the exchange-rate call slow and send concurrent POST
  requests; monitor queued work items and response latency.
- **Recommendation:** Await all asynchronous calls and keep the entire call
  chain asynchronous.
- **Effort:** S

### F-008 — Outbound HTTP has an infinite timeout

- **Severity / priority:** High / P1
- **Location:** `Program.cs`, `AddHttpClient<ExchangeRateClient>`
- **Why it matters:** A stalled dependency can hold request resources
  indefinitely and amplify an upstream incident.
- **Reproduce:** Route the dependency to a server that accepts a connection but
  never responds; the request does not reach a bounded failure.
- **Recommendation:** Set a business-appropriate timeout and add bounded retry
  and circuit-breaker behavior only for safe transient failures.
- **Effort:** S

### F-009 — Missing request validation

- **Severity / priority:** High / P1
- **Location:** `CreateOrderRequest`; `OrdersController.Create`
- **Why it matters:** Empty emails, negative amounts and oversized references
  reach persistence and downstream calls, causing bad data and avoidable 500s.
- **Reproduce:** POST an empty email, a blank reference and `amount: -1`; the
  request proceeds instead of returning `400`.
- **Recommendation:** Add boundary validation, stable error responses and tests
  for invalid and extreme inputs.
- **Effort:** M

### F-010 — Broad exception handling

- **Severity / priority:** Medium / P2
- **Location:** `OrdersController.Create`
- **Why it matters:** A single `catch (Exception)` collapses validation,
  dependency, database and programming failures into the same response, making
  retries and incident diagnosis unreliable.
- **Reproduce:** Trigger both a duplicate-customer database error and an
  exchange-rate failure; both become the same generic `500`.
- **Recommendation:** Handle expected failures explicitly and centralize
  unexpected exception mapping with `ProblemDetails`.
- **Effort:** M

### F-011 — Sensitive values logged

- **Severity / priority:** Critical / P0
- **Location:** `AuthController.CreateToken`; `OrdersController.Create`
- **Why it matters:** Passwords and customer emails enter centralized logs,
  backups and third-party observability systems, expanding breach impact.
- **Reproduce:** Request a token and create an order, then inspect application
  logs.
- **Recommendation:** Never log credentials; minimize or hash identifiers,
  define a logging classification policy and add redaction tests.
- **Effort:** S

### F-012 — Missing lookup index

- **Severity / priority:** Medium / P1
- **Location:** `DemoDbContext.OnModelCreating`; lookup by
  `Order.ExternalReference`
- **Why it matters:** Reference lookup degrades to a table scan as order volume
  grows.
- **Reproduce:** Run `EXPLAIN ANALYZE` for the external-reference predicate on a
  representative dataset.
- **Recommendation:** Add a unique or non-unique index based on the business
  invariant, deploy it safely and verify the query plan.
- **Effort:** S

### F-013 — Layer boundaries are bypassed

- **Severity / priority:** Medium / P2
- **Location:** API controllers reference `DemoDbContext` and domain entities
  directly while `IOrderReader` is unused.
- **Why it matters:** Transport, orchestration and persistence concerns change
  together, increasing test setup and making AI-generated edits less local and
  predictable.
- **Reproduce:** Attempt to change persistence or test order behavior without
  constructing EF Core infrastructure.
- **Recommendation:** Move use cases behind Application interfaces, keep API
  models at the boundary and implement persistence in Infrastructure.
- **Effort:** L

### F-014 — Happy-path-only integration tests

- **Severity / priority:** Medium / P2
- **Location:** `ApiHappyPathTests`
- **Why it matters:** Authorization, validation, cancellation, database failure,
  concurrency and dependency-failure behavior can regress undetected.
- **Reproduce:** Review the two tests; both assert only successful responses.
- **Recommendation:** Add a risk-based matrix covering `401/403`, invalid input,
  not-found, duplicate data, dependency timeout and concurrent requests.
- **Effort:** M

### F-015 — No dependency-aware health checks

- **Severity / priority:** High / P1
- **Location:** application startup and endpoint routing
- **Why it matters:** `/api/status` reports the process as running even when
  PostgreSQL is unavailable. Orchestrators can route traffic to an unready
  instance.
- **Reproduce:** Stop PostgreSQL and call `/api/status`; it still returns `200`.
- **Recommendation:** Add separate liveness and readiness checks, include the
  database in readiness and configure container/orchestrator probes.
- **Effort:** M

## 4. AI/Codex readiness

Positive signals include a small solution, explicit project names, repeatable
commands and an audit report that links risks to code. Readiness is reduced by
controllers that own persistence, implicit runtime assumptions, weak negative
tests and missing operational contracts. Before delegating larger changes to an
AI coding agent, add repository guidance, architectural boundaries, deterministic
test data and a trustworthy CI gate.

## 5. Limitations

This review is based on the supplied code and local execution. Production
infrastructure, runtime traffic, cloud configuration and organizational controls
were not assessed without access. This is not a penetration test and does not
guarantee that every defect or vulnerability was found. Private client code must
not be sent to external AI services without explicit approval.
