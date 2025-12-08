## Feature DoD
- Tests updated/added (unit; integration if a boundary is touched).
- Structured logs at meaningful points (message templates, named properties, log exceptions once at the boundary).
- Docs updated (README/ADR).
- Perf/security considered briefly.
- Migrations safe (if data changed).
- Async public APIs include `CancellationToken` and are suffixed `Async`.
- All classes use **constructor DI** for collaborators (logger, validator, repos, gateways, clock) and wrap method bodies in
  `try/catch` unless an explicit comment explains why not.
- No service locator, no static singletons for shared state.
- Only pure helpers are static.
- Extensibility considered using the decision table:
  - [ ] External boundary behind a port (if applicable)
  - [ ] Real variation → Strategy (only if ≥2 variants now/soon)
  - [ ] Construction differs → Factory (config/env)
  - [ ] Cross-cut → Decorator (logging/caching/metrics)
  - [ ] Otherwise: no pattern (keep it simple)
- One-line note explaining any new seam’s purpose and trigger (why it exists).

If the feature touches an HTTP boundary:

- Errors mapped using `Result` / `Result<T>` + `ErrorCode` via a central helper (e.g. ProblemDetails).
- Error mapping covers Validation (400), Domain (422), Unexpected (500).
- Structured logging includes CorrelationId (TraceIdentifier) + key identifiers in scope.
- Tests cover happy path + invalid input; assert exact HTTP status codes.
- DevKit self-check passed for HTTP code (Async+CT, scope keys, Result+ErrorCode mapping, 400/422/500, full-method try/catch at the boundary with single-point exception logging, tests assert status, no unjustified seams).

---

## PR Review
**Correctness • Boundaries • Tests • Logging • Security • Performance • Docs**

- Over-engineering guard: did we add an interface/class without a real boundary or variation?
- Patterns used match the triggers; otherwise recommend simpler code.

---

## Package guard (dotnet)
- [ ] Does any file reference types from packages not in the `.csproj`?
- [ ] If yes, **add the correct `<PackageReference>` entries to the appropriate `.csproj` and show the updated snippet**.
- [ ] Confirm `Directory.Build.props` has `<ImplicitUsings>enable</ImplicitUsings>`.

---

## Operational Checklist (AI must apply all)

For any new **operational** class (handles input or orchestrates work, e.g. endpoint, handler, saga, worker, CLI):

- [ ] Uses a typed `Result` / `Result<T>` and an `ErrorCode` enum (or equivalent error categories) for all outcomes.
- [ ] Uses `CancellationToken` on async methods and passes it through to async collaborators.
- [ ] Validates input at the edge (DTO/message/command) and treats validation failure as an error outcome (e.g. `ErrorCode.Validation`).
- [ ] Uses a structured logging scope with a correlation/trace identifier when available.
- [ ] No `new` of collaborators inside methods (DI only via constructor).
- [ ] Tests cover: success, validation failure, unexpected error, and cancellation (where supported).
- [ ] Wraps the full method body in `try/catch` per `languages/dotnet/style.md#exception-handling-canonical` and logs exceptions
      once at the operational boundary.

---

## HTTP Endpoint Checklist (AI must apply all)

For any new HTTP endpoint or controller:

- [ ] Maps `Result` / `Result<T>` + `ErrorCode` to HTTP status codes consistently (e.g. Validation → 400, Domain → 422, Unexpected → 500).
- [ ] Uses ProblemDetails (or the project’s mapping helper) for error responses.
- [ ] Uses FluentValidation for request DTOs and converts validation failures into appropriate 4xx responses.
- [ ] Uses a logging scope that includes `HttpContext.TraceIdentifier` as CorrelationId plus key identifiers (e.g. UserId, Email, OrderId).
- [ ] Never `new` services; all dependencies resolved via DI.
- [ ] Tests assert exact HTTP responses for: success, validation failure, domain failure, cancelled, and unexpected errors.

If any item in the Operational or HTTP checklist is not met (where applicable), change the code and/or tests to comply **before** considering the feature complete.
