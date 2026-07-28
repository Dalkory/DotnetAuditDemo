# AI Repo Enablement for .NET — demonstration kit

This synthetic case demonstrates a fixed-scope repository enablement package
for teams that want AI coding agents to produce smaller, safer and more
reviewable pull requests.

The package does not grant an agent unrestricted autonomy. It makes the
repository's build, test, architecture and security boundaries explicit, then
provides bounded example tasks with acceptance checks.

## Deliverables in this case

- [`AGENTS.md`](AGENTS.md) — cross-agent repository rules.
- [`.github/copilot-instructions.md`](.github/copilot-instructions.md) —
  Copilot-specific project context.
- [`SECURITY_BOUNDARIES.md`](SECURITY_BOUNDARIES.md) — forbidden actions,
  secret-handling rules and approval gates.
- [`PREFLIGHT_CHECKLIST.md`](PREFLIGHT_CHECKLIST.md) — readiness and acceptance
  checklist.
- [`agent-tasks/`](agent-tasks/) — five bounded task specifications a team can
  adapt to its own backlog.

## Intended workflow

1. Inventory the repository and identify the real build/test commands.
2. Agree prohibited data, commands and deployment paths with the owner.
3. Add always-on instructions that match the repository rather than generic
   coding advice.
4. Add path-specific rules only where the codebase genuinely needs them.
5. Test the instructions on a small, reversible task.
6. Review the resulting diff, build logs and tests before broadening access.

## Boundaries

This case does not claim that instruction files make an agent safe by
themselves. Branch protection, least-privilege credentials, human review and
repository-specific validation remain necessary.

The commercial package covers one repository and a fixed onboarding scope.
Architecture cleanup, CI remediation, modernization and implementation work are
separate follow-on scopes.
