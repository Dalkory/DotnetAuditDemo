# AI-agent repository preflight

## Repository context

- [ ] Build, test and run commands are checked in and current.
- [ ] Architecture boundaries and owners are documented.
- [ ] Public contracts and compatibility constraints are named.
- [ ] A repository-wide `AGENTS.md` exists.
- [ ] Copilot-specific instructions exist only where they add useful context.

## Safety

- [ ] Secret scanning and ignore rules are enabled.
- [ ] Production credentials are unavailable to routine agent tasks.
- [ ] Protected branches require review.
- [ ] Deployment and release steps require an explicit gate.
- [ ] Private data and external-AI rules are agreed in writing.

## Validation

- [ ] Restore/build/test can run non-interactively.
- [ ] Relevant tests fail when the expected behavior is broken.
- [ ] CI reports actionable output.
- [ ] A small pilot task produced a bounded, understandable diff.
- [ ] The pull-request template captures risk, evidence and rollback.

## Decision

- **Ready:** all mandatory controls are present and the pilot passed.
- **Ready with conditions:** bounded use is acceptable after listed fixes.
- **Not ready:** missing controls allow unreviewed or unverifiable changes.
