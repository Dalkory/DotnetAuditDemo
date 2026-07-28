# Compatibility and delivery blockers

| ID | Priority | Evidence | Why it blocks a direct upgrade | Required decision |
|---|---|---|---|---|
| M-01 | High | `ProjectTypeGuids`, old MSBuild format | Modern tooling and package management expect SDK style | Convert project structure before feature work |
| M-02 | High | `System.Web.Mvc` and `System.Web` references | ASP.NET Core does not provide the `System.Web` request pipeline | Select controller/minimal API target and map filters, sessions, and model binding |
| M-03 | High | Forms Authentication in `Web.config` | Authentication behavior cannot be copied as-is | Select cookie/OIDC/JWT scheme and define authorization acceptance tests |
| M-04 | High | EF6 `DbContext` created by controller | Lifetime and transaction behavior are implicit | Introduce DI and decide EF6 bridge vs EF Core |
| M-05 | Medium | `packages.config` | Dependency compatibility and transitive graph are opaque | Convert to `PackageReference`, update one package family at a time |
| M-06 | Medium | IIS-specific configuration | Local and production hosting assumptions are mixed | Define hosting target, health checks, proxy headers, and configuration sources |
| M-07 | High | No tests in the sample | Behavior can change without evidence | Add characterization tests before porting writes |
| M-08 | Medium | Synchronous database access | Thread usage and cancellation differ in ASP.NET Core | Port query paths to async with cancellation and measurements |

## Non-blocking but important

- Define logging fields that exclude customer secrets and personal data.
- Add a package-vulnerability scan and supported-runtime check to CI.
- Capture a rollback path for each migration slice.
- Keep the legacy and modern applications side by side until acceptance gates pass.
