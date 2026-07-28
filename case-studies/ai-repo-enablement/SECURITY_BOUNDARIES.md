# Security boundaries

## Always prohibited

- Push directly to a protected branch.
- Read or export unrelated credentials, tokens or personal data.
- Add secrets to source, prompts, logs, fixtures or screenshots.
- Disable authorization, validation, security scanning or branch protection to
  make a task pass.
- Execute downloaded scripts or broad destructive commands without review.
- Deploy, publish a package or change production state as a side effect of a
  code task.

## Human approval required

- New external service or data processor.
- Changes to identity, permissions, encryption or secret storage.
- Database migrations with destructive or irreversible effects.
- Deployment, release, package publication or infrastructure changes.
- Access to private code outside the agreed repository and scope.

## Safe default

Use a feature branch, least-privilege credentials, synthetic test data and
reviewable pull requests. Redact values in logs and reports. If the required
authority is ambiguous, produce a plan and request a decision.

## Acceptance evidence

The final pull request should record the changed files, commands executed,
tests, remaining risks and rollback path. Passing tests do not replace human
review of security-sensitive behavior.
