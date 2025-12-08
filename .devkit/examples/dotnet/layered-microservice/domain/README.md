# Domain Layer

**Purpose**
- Capture business rules through entities and value objects.
- Remain free of infrastructure, logging, or validation libraries.

**Golden Rules**
- Accept primitives/value objects, return domain types.
- Throw `DomainRuleException` when invariants break; never reference `Result` here.
- Keep constructors private and expose named factories.
- Avoid time/date retrieval; pass values in from application services.

**Example**
```csharp
public static Order Create(Guid customerId, IEnumerable<OrderLineDraft> lines, DateTimeOffset createdAt)
{
    var orderId = Guid.NewGuid();
    var materialised = lines.Select(l => new OrderLine(l.Sku, l.Quantity)).ToArray();
    if (!materialised.Any())
    {
        throw new DomainRuleException("Order must contain at least one line", orderId);
    }

    if (materialised.Any(line => line.Quantity <= 0))
    {
        throw new DomainRuleException("Line quantity must be positive", orderId);
    }

    return new Order(orderId, customerId, materialised, createdAt);
}
```

**Use this prompt**
> Generate a domain entity factory that enforces invariants with `DomainRuleException` and avoids any infrastructure or logging concerns.
