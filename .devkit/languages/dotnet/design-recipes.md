# Patterns

## Where each recipe lives
| Recipe | Layer | Canonical example |
|---|---|---|
| API endpoint | API | [layered microservice API](../../examples/dotnet/layered-microservice/api/README.md) |
| Validation | API / Application | [request validator](../../examples/dotnet/layered-microservice/api/OrdersController.cs) |
| Service/handler | Application | [create order handler](../../examples/dotnet/layered-microservice/application/CreateOrderHandler.cs) |
| Result + ProblemDetails | Shared | [result primitives](../../examples/dotnet/layered-microservice/shared/README.md) |
| Domain rules | Domain | [order aggregate](../../examples/dotnet/layered-microservice/domain/Order.cs) |
| Infrastructure adapter | Infrastructure | [SQL repository](../../examples/dotnet/layered-microservice/infrastructure/SqlOrderRepository.cs) |
| Tests | Tests | [status mapping tests](../../examples/dotnet/layered-microservice/tests/README.md) |

See [examples/dotnet/layered-microservice](../../examples/dotnet/layered-microservice/) for the canonical layered structure, then return here for focused recipes.
Model your project’s local `Result`, `ErrorCode`, and mapping helpers after the examples in
`examples/dotnet/layered-microservice/shared/`. Do not reference or compile `.devkit` files into your application.


## Extensibility decision table
| Situation | Use | Rationale | Example trigger |
|---|---|---|---|
| Talking to HTTP/DB/Queue/FS | **Port + Adapter** | External boundary; testable seam | Replace gateway, fake in tests |
| Multiple algorithms/variants | **Strategy** | Swap behaviour by type/rule | Payment method, pricing rules |
| Construction differs by env/config | **Factory** | Centralise creation; hide wiring | Sandbox vs prod gateway |
| Add concerns around existing flow | **Decorator** | Add logging/caching/metrics | Cache read models; log at edges |
| Refactor big if/else with data-driven steps | **Template/Policy/Chain** | Stable flow with varying steps | Risk checks, approval chains |

> Default to **no pattern**. Use the simplest thing that meets the need.

See: examples/dotnet/design-recipes/api-endpoint/OrdersController.cs

## Testing recipe (.NET)

- Test framework: **NUnit** + **Moq** + **FluentAssertions**.
- Keep tests in a **separate test project**. Do not place tests in controller files.
- Assert **exact** status codes (400 for validation; 422 for domain failures; 500 only for unexpected).
- Use clear arrange/act/assert; keep tests short and focused.

See: examples/dotnet/design-recipes/api-endpoint/OrdersController_tests.cs

## Class recipe (service/handler)

- Constructor DI all collaborators (logger, validator, clock, gateway, repo).
- Validate inputs early (validator or guard clauses).
- Return `Result/Result<T>` for expected outcomes; throw for unexpected.
- No HTTP types in domain/application layers.

See `examples/dotnet/design-recipes/class-service/PaymentService.cs`

### Composition root (Program.cs)
- Register validators using assembly scanning (FluentValidation).
- Register gateways/repos with appropriate lifetimes.
- Prefer typed clients or factories when the dependency varies per call.

### Dependencies
- Required NuGet: **add `<PackageReference>` entries to the appropriate `.csproj` and show the updated snippet** when code
  depends on a package (do not assume packages already exist). Minimum set:
  - `FluentValidation (>=12.0.0)`
- Required usings in controllers/services:
  - Controllers: `Microsoft.AspNetCore.Mvc`, `Microsoft.Extensions.Logging`, `FluentValidation`, `System.Linq`, `System.Collections.Generic`
  - Services: `Microsoft.Extensions.Logging`, `System.Collections.Generic` (if logging scopes)

### Swagger/OpenAPI
- Include `Swashbuckle.AspNetCore` as a `<PackageReference>` when generating Swagger-enabled APIs and show the `.csproj` snippet.
- In `Program.cs`, wire `AddSwaggerGen()` during service registration and call `UseSwagger()` plus `UseSwaggerUI()` in the
  pipeline.
