# Style

## Exception Handling (canonical)
- Wrap the **entire body of every method** in a `try/catch`.
- If a method deliberately omits `try/catch`, place a comment immediately **above the method** explaining the explicit reason.
- Default boundary pattern:
  ```csharp
  public ReturnType MethodName(Args...)
  {
      try
      {
          // Entire method logic
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Failed to {operationDescription}. {@Args}", ...);
          throw;
      }
  }
  ```
- Log exceptions **once** at the request/handler boundary (controllers, message handlers, job entry points). Inner layers still
  wrap in `try/catch` but rethrow without logging and keep the catch block minimal so the boundary can log.

## Async & Cancellation
- **All public async methods must accept a `CancellationToken`** and pass it to async collaborators.
- Private helper methods may omit the token **only** if they perform no I/O and no long-running work.
- Public async method names end with `Async`.

## Logging
- Structured logging **only** (message templates + named properties); no string interpolation or concatenation.
- Log exceptions **once** at the operational boundary; lower layers throw and let the boundary handle logging.
- Use scopes to attach correlation/trace identifiers and key entity identifiers.

## Naming
- Classes, methods, properties, and events: **PascalCase**.
- Parameters and local variables: **camelCase**.
- Private fields: **_camelCase**.
- Common .NET abbreviations are allowed (e.g., `Dto`, `Api`, `Id`).

## Testing
- Use **NUnit** exclusively as the test framework.
- Prefer **FluentAssertions** for assertions and **Moq** for mocking.
- Name test methods `<Method>_<Condition>_<ExpectedOutcome>` (e.g., `PayAsync_InvalidInput_ReturnsBadRequest`).

## Libraries & Frameworks (pinned)
- Logging: **ILogger<T>** with **Serilog** sink (structured templates only).
- Testing: **NUnit** with **FluentAssertions**; mocks via **Moq**.
- Validation: **FluentValidation** for request/DTO validation.
- HTTP resilience (when needed): **Polly** policies (timeouts, retries with backoff, circuit breaker).

## Packages & Swagger
- When generated code relies on a library that may not already exist in the project, **add `<PackageReference>` entries to the
  relevant `.csproj` and show the updated snippet in the answer**; do not assume packages already exist and do not skip `.csproj`
  edits.
- Swagger/OpenAPI setup must include:
  - `Swashbuckle.AspNetCore` as a `PackageReference`.
  - `AddSwaggerGen()` during service registration plus `UseSwagger()` and `UseSwaggerUI()` in `Program.cs`.

## XML Comments Guidelines
Use XML comments only where they add real value:
- ✅ Public API boundaries (controllers, DTOs, public interfaces)
- ✅ Non-obvious behaviour or invariants
- 🚫 Omit for short private methods and clear domain types
- 🧭 Keep summaries short and imperative ("Processes payment requests.")

## Extensibility heuristics
- **Seams at boundaries**: wrap external I/O in interfaces (ports), implement adapters.
- **Strategy over if/else**: when behaviour switches by type or rule, prefer Strategy.
- **Factory for creation** when construction varies by environment/config (not for every object).
- **Decorator** only when you add cross-cuts (logging, caching, metrics) without changing core behaviour.
- Each new interface/class must pass: "What **concrete variation** or **external boundary** requires this?"

## DI across the codebase
- Register all concrete implementations in the composition root (e.g., `Program.cs` / `Startup`).
- Inject **ILogger<T>**, **IValidator<T>**, gateways, and repositories into **any** class that needs them (not only controllers).
- Avoid `new Foo()` for collaborators; prefer `IFoo` injected.
- No `IServiceProvider` plumbing in domain/application code (no service locator).

## Avoid
- `new`ing collaborators in methods/constructors (use DI instead).
- `IServiceProvider` in domain/application layers (no service locator).
- Static mutable state; prefer DI + lifetimes.
