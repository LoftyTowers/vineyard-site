# Operational Contract

## Instruction Precedence

Follow instructions in this order:

1. C# / .NET language and runtime rules
2. Specific user instructions in the current task
3. Project-specific DevKit overrides
4. DevKit rules in this repository
5. Model defaults

All files in the DevKit must defer to this precedence. If another document lists a different order, replace it with a reference
to this section.

This contract applies to any code that reacts to input and orchestrates work:

- HTTP controllers / endpoints
- NServiceBus handlers and sagas
- message consumers
- background workers / scheduled jobs
- CLI commands that perform operations

Treat all of these as **operational handlers** and follow the rules below.

---

## 1. Dependency injection

- All collaborators (repositories, gateways, clients, clocks, validators, loggers, etc.) must be injected via the constructor.
- Do **not** resolve services manually or use service locators inside methods.
- Do **not** call `new` for collaborators inside operational methods.
- Static members are only allowed for **pure, stateless helpers** (no I/O, no shared state).

---

## 2. Async and cancellation

- Operational methods must be `async` when the platform allows it and **public async methods must accept a `CancellationToken`**.
- Always pass the token through to async collaborators and check it before heavy or long-running work.
- When cancelled, treat it as a **first-class outcome** (log at the boundary and return/emit a clear cancellation result).

---

## 3. Outcome and error handling

Operational code must never leak unhandled exceptions as normal behaviour.

- Normal outcomes should use a typed result or similar pattern, with:
  - `Success` / `IsSuccess`
  - a small set of error categories (e.g. a local `ErrorCode` enum: Validation, Domain, Cancelled, Unexpected)
  - optional error messages or reasons.
- Use exceptions only for unexpected conditions, not for validation or known domain rules.
- Wrap the **entire body** of each operational method in `try/catch` using the canonical rule in
  `languages/dotnet/style.md#exception-handling-canonical`.
- Exceptions are logged **once** at the operational boundary (controller, handler, job entry point). Inner layers rethrow and
  let the boundary log.

Transport-specific rules:

- **HTTP endpoints**:
  - Map result + error category to HTTP responses via a central mapping (e.g. ProblemDetails, status code).
  - Do not create ad-hoc status codes per endpoint.
- **Non-HTTP handlers (sagas, workers, consumers)**:
  - Use result-like outcomes, events, or state transitions to represent failure.
  - Do not rely on callers catching exceptions as the normal error signal.

---

## 4. Validation

- Perform input validation at the **edge**:
  - HTTP: use FluentValidation on request DTOs.
  - Messaging: validate incoming messages before acting.
  - Workers/CLI: validate arguments before doing work.
- A validation failure is an **error outcome**, not just a flag on a success:
  - HTTP: return a 4xx response with a structured body.
  - Non-HTTP: return or emit a result/event that clearly states validation has failed.
- Do not duplicate the same validation rules in multiple layers without a reason.
  - If a validator guarantees basic shape (e.g. email syntax), downstream handlers may assume that shape.

---

## 5. Logging and scopes

- Use structured logging (`ILogger<T>` or equivalent) with message templates and named properties.
- For each operation, open a logging scope that includes:
  - correlation / trace id (if the platform provides one),
  - key identifiers (OrderId, Email, UserId, etc.).
- Log exceptions **once** at the boundary; lower layers throw and allow the boundary to log with context.
- Log start/end of significant operations at **Information** level and validation/domain failures at **Information** or
  **Warning**.

---

## 6. Separation of concerns

- Operational code orchestrates work; it does **not** own core domain rules.
- Keep:
  - HTTP / messaging / worker concerns in the operational layer,
  - business rules in domain/services,
  - persistence in infrastructure.
- Do not let sagas, controllers, or workers become “god objects” that do validation, business rules, and persistence in one place.

---

## 7. Testing expectations

For any new operational handler (controller, saga, consumer, worker):

- [ ] Tests cover the **happy path**.
- [ ] Tests cover at least one **validation failure**.
- [ ] Tests cover at least one **unexpected error path** from a collaborator.
- [ ] Where cancellation is supported, tests cover cancelled work.
- [ ] Tests assert behaviour (outcome, events, status), not internal implementation details.

If any of these are missing, update the code or tests until this checklist passes.
