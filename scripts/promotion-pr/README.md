# Promotion PR generator

Maintains a long-lived promotion PR between environment branches and keeps the body up to date based on merged PRs.

## Flows

Dev → Staging:

```bash
BASE_BRANCH=staging \
HEAD_BRANCH=dev \
PR_TITLE="Promote: dev → staging" \
PR_LABELS="promotion,automation,env:staging" \
./scripts/promotion-pr/generate-promotion-pr-body.sh
```

Staging → Main:

```bash
BASE_BRANCH=main \
HEAD_BRANCH=staging \
PR_TITLE="Release: staging → main" \
PR_LABELS="promotion,automation,env:prod,release" \
./scripts/promotion-pr/generate-promotion-pr-body.sh
```

## Labels and meanings
- promotion: Promotes changes between environment branches
- automation: Managed by automation
- env:staging: Targets the staging environment
- env:prod: Targets the production environment
- release: Production release candidate
- feature: Feature change
- bug: Bug fix
- tech-debt: Maintenance or refactor
- infra: Infrastructure or platform changes
- docs: Documentation changes
- breaking: Breaking changes

GitHub labels are repo-scoped. The script will auto-create required labels if they are missing.

## Run locally
Requires `gh` and authentication.

```bash
export REPO=owner/repo
export GH_TOKEN=your_token
./scripts/promotion-pr/generate-promotion-pr-body.sh
```
