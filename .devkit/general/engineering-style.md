# Engineering Style (language-agnostic)

- Keep code simple within the DevKit architecture (KISS); avoid speculative hooks (YAGNI).
- Boundaries: high cohesion, low coupling; explicit interfaces at seams.
- Async: suffix Async; **all public async methods accept a CancellationToken** and pass it through.
- Errors: wrap each method body in `try/catch` (see `languages/dotnet/style.md#exception-handling-canonical`); return domain
  results internally; never swallow exceptions.
- Validation: declarative rules near the DTO; fail fast; no exceptions for control flow.
- Immutability where possible for inputs/DTOs.
- Docs light but up to date; ADRs for notable decisions.
- Prefer async all the way: `ValidateAsync`, async gateways/repos, pass `CancellationToken` end-to-end.
- Controllers never fabricate business IDs; services own ID creation.
- Centralise HTTP mapping via helper; no ad-hoc `Problem(...)` in controllers; log exceptions once at the operational boundary.

## SOLID (pragmatic)
S - one reason to change per unit.
O - add behaviour via new types, not giant switches.
L - keep contracts unsurprising.
I - small, focused interfaces.
D - depend on abstractions at seams; concrete inside modules if simpler.

## Extensibility heuristics
Add a seam only if:
1) It's an external dependency (HTTP/DB/Queue/FS), or
2) There are >= 2 real variants now/soon, or
3) Product needs plug-ins/feature flags.

## Dependency Injection (applies to ALL classes)

- Take dependencies via **constructor injection**. No `new` inside classes except:
  - **Pure values/DTOs** (e.g., `new Money(…)`, `new RequestDto(…)`)
  - **In-method short-lived** data structures (e.g., `new List<T>()`)
- Do **not** use service locators or statics for mutable/shared state.
- Prefer **interfaces** at boundaries (IO, time, random, crypto, env/config, HTTP, DB).
- Lifetime guidance (defaults):
  - **Singleton**: stateless pure services, mappers, clocks.
  - **Scoped**: per request/operation (units of work, DbContext).
  - **Transient**: cheap, stateless helpers.
- Factories only when the dependency **varies per call** (e.g., keyed clients, multi-tenant).
- If you add a seam, include a **one-line reason** in code: `// seam: two providers now`.


## Documentation / XML comments (pragmatic)
Write XML/structured comments when:
- You are publishing a **public library/SDK** consumed by other teams or clients.
- A **public API surface** is used across bounded contexts and discoverability matters (e.g., shared domain packages).
- You generate **Swagger/OpenAPI** docs and want rich descriptions (summary/remarks) in the UI.

Do **not** write XML comments when:
- The code is app-internal and names are already clear.
- The comment would merely restate the method name/signature.

When you do write them:
- Keep to **`<summary>`**, **`<param>`**, **`<returns>`**, **`<remarks>`**, **`<example>`**—short and specific.
- Document **behaviour**, side effects, and **invariants**; not “what the code does line-by-line”.
- Prefer **tests** and **clear naming** over verbose comments.


For any seam added, add a one-line reason in code.
