Include preludes/prelude-general.md, then apply .NET specifics:
- languages/dotnet/style.md
- languages/dotnet/libraries.md
- languages/dotnet/design-recipes.md

## Workflow you must follow when generating .NET code

Operational classes (HTTP endpoints, handlers, sagas, workers, CLI commands) must follow
`general/operational-contract.md`. Domain entities, value objects, pure utilities, repositories,
and configuration types follow general style guidance but do not use operational rules.

For any non-trivial piece of code (endpoint, service, repository, background job, etc.):

1. **Locate relevant DevKit rules and examples**:
   - Look in `.devkit/languages/dotnet/design-recipes.md`.
   - Look in `.devkit/examples/dotnet/...`.

2. **Infer the pattern by analogy**:
   - Identify which example is closest in structure or purpose.
   - Keep the same layering:
     - API → Application/Domain → Infrastructure
     - Result/ErrorCode for outcomes
     - Validation → Result with ErrorCode.Validation (not flags)

3. **Generate the code**:
   - Follow the shape, naming, and error flow of the chosen example.
   - Avoid adding ad-hoc behaviour that is not present anywhere in the DevKit.

4. **Self-check against DevKit**:
   - After generating, scan the code and ask:
     - Does it use Result/ErrorCode like the examples?
     - Does it apply cancellation tokens like the examples?
     - Does it log and scope like the examples?
     - Would this fit naturally alongside `.devkit/examples/dotnet/...`?

If the answer to any self-check is “no”, rewrite the code to match the DevKit style.

### Layer Rules Quick Check
| Layer | Guard rails |
|---|---|
| API | Validate with FluentValidation, open logging scope with correlation, map results via `ToActionResult()`, catch cancellation to return 499. |
| Application | Validate commands, invoke domain factories, call ports, translate port failures to `Result<T>`, log context but no HTTP types. |
| Domain | Pure business rules only, throw `DomainRuleException` on invariant breach, no logging/IO/time lookups. |
| Infrastructure | Implement ports, map external failures to `ErrorCode` before returning, never embed domain rules. |
| Shared | Keep primitives/framework helpers (`Result`, `ErrorCode`, `IClock`, mapping extensions) consistent across layers. |
| Tests | Use NUnit + Moq, cover happy/error/cancellation, assert exact HTTP codes via `ToActionResult()`. |

## Usings & packages (do this automatically)
- Always include required `using` statements explicitly in generated files:
  - `Microsoft.AspNetCore.Mvc`, `Microsoft.Extensions.Logging`, `FluentValidation`,
    `System.Linq`, `System.Collections.Generic`.
- When code requires a library, **add the appropriate `<PackageReference>` entries to the correct `.csproj` and show the
  updated snippet**. Do not assume the package exists and do not replace this with install commands.
- Use FluentValidation **12+**. Prefer manual edge validation via `ValidateAsync(..., CancellationToken)`.
- No `FluentValidation.AspNetCore` unless you also wire MVC auto-validation. Manual validation is the default here.

### Style Rules Quick Check
- Follow the [XML Comments Guidelines](../languages/dotnet/style.md#xml-comments-guidelines); only document public APIs or nuanced behaviour.
- Public async APIs: name ends with Async and accept CancellationToken.
- Wrap every method body in `try/catch` per `languages/dotnet/style.md#exception-handling-canonical`; log exceptions once at the
  operational boundary.
- BeginScope includes CorrelationId and key ids (OrderId/PaymentId).
- All failures use `Result<T>.Failure(code, errors)`.
- Map errors: 400 Validation, 422 Domain, 500 Unexpected at edges.
- Controller edge follows the full-method `try/catch` rule (boundary handles logging and rethrows).
- Tests assert exact status codes.
- No new seam unless a trigger applies; add a one-line reason if you add one.

### DI compliance (all classes)
- Use constructor DI for ILogger<T>, IValidator<T>, repositories, gateways, clocks, config.
- No `IServiceProvider.GetService` in domain/application code.
- If a dependency varies per call, introduce a small factory interface (e.g., `IPaymentGatewayFactory`) and inject that.

### Validation (must follow)
- Always call `ValidateAsync(request, cancellationToken)` (no synchronous `Validate`).
- Use injected `IValidator<T>`; never `new` validators in methods.

### HTTP mapping (must follow)
- Never return `Result` types directly to clients.
- Always map via `ResultExtensions.ToActionResult()` so `ErrorCode.Validation` → 400, `ErrorCode.Domain` → 422, `ErrorCode.Cancelled` → 499, and `ErrorCode.Unexpected` → 500.

### ErrorCode Enum Convention

All failures should use `ErrorCode` enum values instead of string literals:

| Enum Name  | HTTP Code | Meaning |
|-------------|------------|----------|
| Validation  | 400        | Input or business rule validation failure |
| Domain      | 422        | Domain or rule violation within business logic |
| Cancelled   | 499        | Request cancelled by client or timeout |
| Unexpected  | 500        | Unhandled or unexpected errors |

Use:
```csharp
Result.Failure(ErrorCode.Validation, "Amount must be greater than zero.")
```

### IDs & scope (must follow)
- Controller must not fabricate IDs (e.g., `PaymentId`). If not present in request, omit it from `BeginScope`.
- Service/domain is responsible for generating business IDs (e.g., PaymentId).

### Cancellation policy
- On `OperationCanceledException`, return **499** by default (`ErrorCode.Cancelled`).
- If your organisation does not allow 499, you may return **400**, but you must:
  - keep this consistent across all controllers/endpoints,
  - keep tests consistent with the chosen status.

### Testing compliance (must follow)
- Generate NUnit tests in a separate test project.
- Use Moq + FluentAssertions.
- Assert exact HTTP status codes (400/422/500).
- Mirror structure from: examples/dotnet/design-recipes/api-endpoint/OrdersController_tests.cs.

Before outputting code, run the DevKit self-check and confirm tests meet the policy.

Pinned stack: ILogger<T> with Serilog, NUnit + Moq + FluentAssertions, FluentValidation, Polly (when needed).
Use CancellationToken in public async APIs; suffix Async. Map failures to ProblemDetails.
Where an example is referenced, open the file from /examples/dotnet/... and mirror that pattern.
