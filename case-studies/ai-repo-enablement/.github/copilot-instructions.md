# Copilot repository instructions

Work in small, reviewable increments. Read the root `AGENTS.md` before making
changes and treat it as the cross-agent source of truth.

The repository is a .NET solution. Prefer the existing SDK, package versions,
code style and test framework. Do not introduce a library merely to shorten a
small implementation.

Before editing, identify the affected project, its direct tests and any public
contract. After editing, run the narrowest relevant tests followed by the
repository build. Never claim validation that was not executed.

Do not change authentication, authorization, secrets, CI permissions,
deployment destinations, payment flows, database schemas or public API
contracts unless the task explicitly names that change and requires human
review.

When information is missing, stop at a documented plan or open question rather
than inventing business behavior.
