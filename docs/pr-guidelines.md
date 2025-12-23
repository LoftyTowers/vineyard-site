# Pull Request Guidelines

Use the repo PR template and fill every section. The goal is quick review and clear release notes.

## Summary
Explain the change in 1-2 sentences.

Example:
> Adds a version selector to the About page editor so admins can preview historical content.

## Release Notes
Write a single sentence suitable for staging/release notes.

Example:
> Adds About page version preview dropdown for admins.

## Changes
Bullet the key changes.

Example:
- Added version dropdown and preview banner to About edit page.
- Added API calls for published page version summaries.

## Risks / Impact
Call out risk or user impact.

Example:
- Low risk; UI-only change behind admin role.

## Test Evidence
List commands run and results.

Example:
- `dotnet test Vineyard.sln` (pass)
- `npm test -- --watch=false --browsers=ChromeHeadless` (pass)

## Screenshots / Evidence
Add screenshots or links when UI changes.

Example:
- Screenshot of About page editor with version dropdown.

## Rollback Plan
Always fill this section.

Example:
- Revert PR #123

## Checklist
Tick what you ran/checked:
- Lint
- Tests
- Build
- Logging checked
- Migrations checked (if applicable)

## Labels
Apply at least one label:
- feature
- bug
- tech-debt
- infra
- docs
- breaking

## Branching rule
All changes into `dev` must come from PRs. No direct pushes.
