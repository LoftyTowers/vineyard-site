Set-Location 'G:\Programming\devkit'

# Ensure folders exist
$dirs = @(
  'general',
  'languages\dotnet',
  'languages\python',
  'examples\dotnet\patterns\factory',
  'examples\dotnet\patterns\strategy',
  'examples\dotnet\patterns\decorator',
  'examples\dotnet\design-recipes\api-endpoint',
  'examples\dotnet\design-recipes\validation',
  'examples\dotnet\design-recipes\problem-details',
  'preludes'
)
foreach ($d in $dirs) { New-Item -ItemType Directory -Force -Path $d | Out-Null }

# GENERAL: engineering-style.md (ASCII-only: use >= instead of ≥)
$engineering = @'
# Engineering Style (language-agnostic)

- Keep code simple (KISS); avoid speculative hooks (YAGNI).
- Boundaries: high cohesion, low coupling; explicit interfaces at seams.
- Async: suffix Async; accept cancellation tokens/handles in public async APIs.
- Errors: handle at edges; return domain results internally; never swallow exceptions.
- Validation: declarative rules near the DTO; fail fast; no exceptions for control flow.
- Immutability where possible for inputs/DTOs.
- Docs light but up to date; ADRs for notable decisions.

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

For any seam added, add a one-line reason in code.
'@
Set-Content -Path .\general\engineering-style.md -Value $engineering -Encoding UTF8

# GENERAL: coding-patterns.md
$patterns = @'
# Coding Patterns (language-agnostic)

## When to add a seam (decision table)
| Situation                         | Use                     | Why                                      |
|-----------------------------------|-------------------------|------------------------------------------|
| External I/O (HTTP/DB/Queue/FS)   | **Port + Adapter**      | Testable boundary; swap provider         |
| >= 2 real variants now/soon       | **Strategy**            | Replace big if/else with swappable code  |
| Construction varies by env/config | **Factory**             | Centralise creation; hide wiring         |
| Cross-cut around behaviour        | **Decorator**           | Add logging/caching/metrics without touching core |
| Data-driven steps                 | **Chain/Policy/Template** | Stable flow with pluggable steps       |

> Default to no pattern. Cite the trigger when you do use one.

## Strategy
Use when behaviour varies by type/rule/flag. Avoid if only one variant and no near-term need.

## Factory
Use when creation depends on config/env and is likely to change.

## Decorator
Use for cross-cuts: logging, caching, metrics, auth.

## Ports & Adapters
Wrap external I/O; keep domain ignorant of transport/provider.
'@
Set-Content -Path .\general\coding-patterns.md -Value $patterns -Encoding UTF8

# GENERAL: design-recipes.md
$recipes = @'
# Design Recipes (technology-neutral)

## Endpoint recipe
- Keep controllers/edges thin; delegate to a handler/service.
- Validate input at the boundary.
- Domain returns Result values; map to transport errors at the edge.
- Add scoped, structured logs (correlation id + key identifiers).

## Validation flow
- Declarative rules near DTO.
- Fail fast; aggregate errors for 400 responses; don’t throw for control flow.

## Result -> ProblemDetails mapping
- Map Validation -> 400, domain failure -> 422, unexpected -> 500.
- Include an error code so mapping is deterministic.

## Observability
- Log meaningful events (start/end, external I/O).
- Use scopes to attach correlation id and entity ids.
- (Optional) metrics/tracing if the platform supports it.
'@
Set-Content -Path .\general\design-recipes.md -Value $recipes -Encoding UTF8

# DOTNET: libraries.md
$libs = @'
# .NET Libraries (Pinned)

- Logging: ILogger<T> with Serilog (structured templates).
- Testing: NUnit + Moq + FluentAssertions.
- Validation: FluentValidation.
- Resilience: Polly where idempotent (timeouts, retry/backoff, circuit breaker).
- HTTP: HttpClientFactory; no `new HttpClient()`.
- Config: Options pattern; validate on start; no secrets in code or logs.
'@
Set-Content -Path .\languages\dotnet\libraries.md -Value $libs -Encoding UTF8

# PRELUDES
$preludeGeneral = @'
Apply the general rules:
- general/charter.md
- general/engineering-style.md
- general/coding-patterns.md
- general/design-recipes.md
- general/checklists.md

When designing:
1) Prefer the simplest code first (KISS). Avoid speculative seams (YAGNI).
2) Consult the decision table in coding-patterns.md. Only add a pattern if a trigger applies.
3) If a pattern is chosen, cite the trigger and add a one-line reason in code comments.

Output for any task:
- summary + trade-offs,
- minimal patch,
- tests,
- notes (logging/error/docs/perf).
'@
Set-Content -Path .\preludes\prelude-general.md -Value $preludeGeneral -Encoding UTF8

$preludeDotnet = @'
Include prelude-general.md, then apply .NET specifics:
- languages/dotnet/style.md
- languages/dotnet/libraries.md
- languages/dotnet/design-recipes.md

Pinned stack: ILogger<T> with Serilog, NUnit + Moq + FluentAssertions, FluentValidation, Polly (when needed).
Use CancellationToken in public async APIs; suffix Async. Map failures to ProblemDetails.
Where an example is referenced, open the file from /examples/dotnet/... and mirror that pattern.
'@
Set-Content -Path .\preludes\prelude-dotnet.md -Value $preludeDotnet -Encoding UTF8

Write-Host "Done. Files written."
