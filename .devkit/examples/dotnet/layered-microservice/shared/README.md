# Shared Layer

**Purpose**
- Provide cross-layer primitives like `Result`, `ErrorCode`, and shared abstractions such as `IClock`.
- Define mapping helpers that keep API responses consistent across services.

**Golden Rules**
- Keep types framework-agnostic where possible; only mapping extensions target ASP.NET Core.
- Enforce the canonical `ErrorCode` values and `GetTitle()` naming.
- Offer `Result` factories that avoid null collections.
- Add lightweight abstractions (`IClock`) for deterministic testing.

**Example**
```csharp
public static IActionResult ToActionResult(this Result result) => result switch
{
    { IsSuccess: true } => new OkResult(),
    { Code: ErrorCode.Validation, Errors: var errors } => new BadRequestObjectResult(new ProblemDetails
    {
        Status = (int)ErrorCode.Validation,
        Title = ErrorCode.Validation.GetTitle(),
        Detail = string.Join("; ", errors)
    }),
    { Code: ErrorCode.Domain, Errors: var domain } => new ObjectResult(new ProblemDetails
    {
        Status = (int)ErrorCode.Domain,
        Title = ErrorCode.Domain.GetTitle(),
        Detail = string.Join("; ", domain)
    }) { StatusCode = 422 },
    { Code: ErrorCode.Cancelled } => new StatusCodeResult(499),
    _ => new ObjectResult(new ProblemDetails { Status = 500, Title = ErrorCode.Unexpected.GetTitle() }) { StatusCode = 500 }
};
```

**Use this prompt**
> Generate shared `Result`/`ErrorCode` helpers (including ASP.NET Core mapping) for the layered microservice example.
