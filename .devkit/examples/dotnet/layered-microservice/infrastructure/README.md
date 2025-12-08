# Infrastructure Layer

**Purpose**
- Adapt external systems (databases, queues, HTTP clients) to application ports.
- Translate external failures into `Result` instances with precise `ErrorCode`s.

**Golden Rules**
- Keep the boundary thin: map from domain types to transport-friendly data and back.
- Catch and classify external exceptions (validation vs unexpected) before returning.
- Log operational failures with identifiers but never embed domain rules.
- Do not leak infrastructure exceptions above the port; return `Result` instead.

**Example**
```csharp
public async Task<Result> SaveAsync(Order order, CancellationToken ct)
{
    try
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "insert into orders ...";
        command.AddParameter("@id", order.Id);
        await command.ExecuteNonQueryAsync(ct);
        return Result.Success();
    }
    catch (DbException ex) when (ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogWarning(ex, "Duplicate order {OrderId}", order.Id);
        return Result.Failure(ErrorCode.Domain, new[] { "Duplicate order" });
    }
    catch (DbException ex)
    {
        _logger.LogError(ex, "Database failure for order {OrderId}", order.Id);
        return Result.Failure(ErrorCode.Unexpected, new[] { "Database failure" });
    }
}
```

**Use this prompt**
> Generate an infrastructure adapter for a port that maps external exceptions to `Result` codes, following the layered microservice rules.
