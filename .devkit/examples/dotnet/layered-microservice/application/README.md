# Application Layer

**Purpose**
- Orchestrate use-cases by validating commands and calling domain logic.
- Coordinate ports (repositories, gateways) and translate failures into `Result` codes.

**Golden Rules**
- Always use `ValidateAsync(...)` with a `CancellationToken` on every public method.
- Keep logic thin: validate, call domain, persist via ports.
- Convert port failures into `ErrorCode` values before returning.
- Log with correlation and entity identifiers via `BeginScope`; no infrastructure access here.

**Example**
```csharp
public async Task<Result<Order>> HandleAsync(CreateOrderCommand command, CancellationToken ct)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object?>
    {
        ["CorrelationId"] = command.CorrelationId,
        ["CustomerId"] = command.CustomerId
    });

    var validation = await _validator.ValidateAsync(command, ct);
    if (!validation.IsValid)
    {
        return Result<Order>.Failure(ErrorCode.Validation, validation.Errors.Select(e => e.ErrorMessage));
    }

    var order = Order.Create(
        command.CustomerId,
        command.Lines.Select(line => new OrderLineDraft(line.Sku, line.Quantity)),
        _clock.UtcNow);

    using var orderScope = _logger.BeginScope(new { command.CorrelationId, OrderId = order.Id });

    var saveResult = await _repository.SaveAsync(order, ct);
    if (!saveResult.IsSuccess)
    {
        _logger.LogError("Persistence failed for {OrderId}", order.Id);
        return Result<Order>.Failure(saveResult.Code ?? ErrorCode.Unexpected, saveResult.Errors);
    }

    return Result<Order>.Success(order);
}
```

**Use this prompt**
> Generate an application service handler that validates a command, invokes domain creation, calls a port, and returns `Result<T>` codes per the layered microservice rules.
