# Commit Message Rules

Use Conventional Commits:

`type(scope): subject`

## Allowed types
- feat
- fix
- refactor
- chore
- docs
- test
- perf
- ci
- build
- revert

## Subject rules
- imperative, lower case
- no trailing period
- max 72 characters

## Suggested scopes
- api
- frontend
- db
- ui
- infra
- ci
- docs
- auth

## Good examples
- `feat(ui): add version selector to about page`
- `fix(api): return 404 for missing page`
- `chore(ci): add commitlint workflow`

## Bad examples (and why)
- `Added new feature` (missing type/scope and subject format)
- `feat: Add new feature.` (subject not lower case, trailing period)
- `fix(ui): this subject is way too long because it exceeds the maximum allowed length for commit headers` (too long)

## How to fix a commit message
- Amend the last commit:
  - `git commit --amend -m "fix(ui): adjust button layout"`
- Reword older commits (interactive rebase):
  - `git rebase -i HEAD~3`
  - Change `pick` to `reword`, save, and edit messages
- If your branch is shared, coordinate before force-pushing.
