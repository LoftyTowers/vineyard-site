Before generating or editing code, read and follow:

- general/engineering-style.md
- general/coding-patterns.md
- general/house-style-contract.md
- general/charter.md
- general/design-recipes.md
- general/operational-contract.md
- general/checklists.md


### When designing:

1) Prefer the simplest DevKit-style solution first.

   “Simple” does NOT mean generic tutorial code.

   “Simple” means:
   - follow the DevKit structure (Result/ErrorCode, validation, DI, logging scopes),
   - keep functions small and readable,
   - avoid unnecessary abstractions UNTIL a DevKit trigger applies.

   If the DevKit has an example in a similar area, copy its structure and adapt it.
   Never fall back to global/default coding patterns when the DevKit offers a precedent.

2) Only add patterns (factory/strategy/decorator/etc.) when a trigger in
   `general/coding-patterns.md` applies. Avoid speculative seams (YAGNI).

3) If you introduce a pattern:
   - cite the trigger in a short comment,
   - copy the closest DevKit example and adapt it.

Operational classes include:

- HTTP endpoints/controllers
- NServiceBus handlers and sagas
- message consumers
- background workers
- CLI/command handlers
- top-level orchestrators in the application layer

Do NOT apply these rules to:

- domain entities
- value objects
- pure utilities
- repositories
- configuration/option objects
- EF Core DbContexts
- infrastructure models

Operational classes must use the following:

- dependency injection,
- async and cancellation,
- logging and scopes,
- validation,
- error handling,
- and testing.

### DI Policy (operational classes)
- Operational classes must follow the DI rules in `general/operational-contract.md`. Other classes may use constructor injection where useful but are not required to follow operational DI rules.
- Static helpers are allowed only when pure. Domain/value objects and simple utility classes
- should remain simple and should not be forced through operational DI rules.

After generating new code, you must mentally apply the "New Feature Checklist" in `general/checklists.md`.
If any item would fail, you must adjust the code so that all items pass.
