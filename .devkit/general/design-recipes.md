# Design Recipes (technology-neutral)

## When to add a seam
| Trigger | Add a seam via |
|---|---|
| Crossing an IO boundary (HTTP, DB, queue, filesystem) | Port + Adapter |
| Two or more real variants need swapping at runtime | Strategy or Factory |
| Optional behaviour toggled by feature flag or policy | Decorator |
| Integration owned by another team/vendor | Port + Adapter |

See also: examples under `examples/dotnet/patterns/` and the layered microservice walkthrough.
Use the shared primitives under `examples/dotnet/layered-microservice/shared` as the **canonical pattern** for `Result`, `ErrorCode`, and mapping helpers.

When you need these concepts in a project:
- define them locally in that project’s own namespaces,
- shape them after the DevKit examples,
- do **not** reference or compile `.devkit` files directly.


## Endpoint recipe
- Keep controllers/edges thin; delegate to a handler/service.
- Validate input at the boundary.
- Domain returns Result values; map to transport errors at the edge.
- Add scoped, structured logs (correlation id + key identifiers).
See: examples/dotnet/design-recipes/api-endpoint/OrdersController.cs

## Validation flow
- Declarative rules near DTO.
- Fail fast; aggregate errors for 400 responses; don't throw for control flow.
See: examples/dotnet/design-recipes/validation/PayRequestValidator.cs

## Result -> ProblemDetails mapping
- Map Validation -> 400, domain failure -> 422, unexpected -> 500.
- Include an error code so mapping is deterministic.
See: `examples/dotnet/layered-microservice/shared/ResultExtensions.cs`

## Observability
- Structured logging only (message templates + named properties).
- Log meaningful events (start/end, external I/O) and keep exception logging at the operational boundary.
- Use scopes to attach correlation id and entity ids.
- (Optional) metrics/tracing if the platform supports it.
