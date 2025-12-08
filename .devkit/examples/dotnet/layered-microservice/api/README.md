# API Layer

**Purpose**
- Expose application workflows via HTTP without leaking domain or infrastructure concerns.
- Validate requests, maintain logging scopes, and translate `Result` outcomes to HTTP responses.

**Golden Rules**
- Always use `ValidateAsync(...)` with a `CancellationToken` via `IValidator<T>`.
- Open a logging scope with correlation and entity identifiers.
- Do not craft `ProblemDetails` manually; call `ToActionResult()` on `Result`/`Result<T>`.
- Map `OperationCanceledException` to `ErrorCode.Cancelled (499)` and log unexpected failures.

**Example**
```csharp
[HttpPost]
public async Task<IActionResult> CreateAsync(CreateOrderRequest request, CancellationToken ct)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object?>
    {
        ["CorrelationId"] = HttpContext.TraceIdentifier,
        ["CustomerId"] = request.CustomerId
    });

    var validation = await _validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
    {
        return Result.Failure(ErrorCode.Validation, validation.Errors.Select(e => e.ErrorMessage)).ToActionResult();
    }

    try
    {
        var result = await _handler.HandleAsync(request.ToCommand(HttpContext.TraceIdentifier), ct);
        return result.ToActionResult(order => new { order.Id, order.Total });
    }
    catch (OperationCanceledException)
    {
        return Result.Failure(ErrorCode.Cancelled, Array.Empty<string>()).ToActionResult();
    }
}
```

**Use this prompt**
> Generate an ASP.NET Core controller action that validates with FluentValidation, uses `ToActionResult`, and respects the layered microservice API rules.
