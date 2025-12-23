# Dev → Staging Release PR automation

This workflow keeps a single PR open from `dev` to `staging` and updates its body based on PRs merged into `dev` but not yet in `staging`.

## How it works
- On every push to `dev` (and on manual trigger), the workflow:
  - Collects commits in `staging..dev`
  - Finds associated PRs
  - Extracts release notes and optional migration/test notes
  - Updates or creates a PR titled `Release: dev → staging`

## Run locally
Requires `gh` and authentication.

```bash
export REPO=owner/repo
export GH_TOKEN=your_token
./scripts/promotion-pr/generate-promotion-pr-body.sh
```

## Labels for grouping
Apply labels on PRs to improve grouping in the release PR:
- breaking
- feature
- bug
- tech-debt
- infra
- docs

Unlabeled or other labels appear under **Other**.
