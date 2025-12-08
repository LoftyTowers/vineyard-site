using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredMicroservice.Domain;

public sealed class Order
{
    private Order(Guid id, Guid customerId, IReadOnlyCollection<OrderLine> lines, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerId = customerId;
        Lines = lines;
        CreatedAt = createdAt;
        Total = lines.Sum(line => line.Quantity);
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public IReadOnlyCollection<OrderLine> Lines { get; }
    public DateTimeOffset CreatedAt { get; }
    public int Total { get; }

    public static Order Create(Guid customerId, IEnumerable<OrderLineDraft> lines, DateTimeOffset createdAt)
    {
        var orderId = Guid.NewGuid();
        var lineArray = lines.Select(l => new OrderLine(l.Sku, l.Quantity)).ToArray();
        if (lineArray.Length == 0)
        {
            throw new DomainRuleException("Order must contain at least one line", orderId);
        }

        if (lineArray.Any(line => line.Quantity <= 0))
        {
            throw new DomainRuleException("Line quantity must be positive", orderId);
        }

        return new Order(orderId, customerId, lineArray, createdAt);
    }
}

public sealed record OrderLine(string Sku, int Quantity);

public sealed record OrderLineDraft(string Sku, int Quantity);

public sealed class DomainRuleException : Exception
{
    public DomainRuleException(string message, Guid orderId) : base(message)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
