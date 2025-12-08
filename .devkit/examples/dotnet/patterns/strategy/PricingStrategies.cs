public interface IPricingStrategy { decimal Price(Order o); }

public sealed class StandardPricing : IPricingStrategy { public decimal Price(Order o) => o.Base; }
public sealed class BlackFridayPricing : IPricingStrategy { public decimal Price(Order o) => o.Base * 0.7m; }

public sealed class PricingService
{
    private readonly IPricingStrategy _strategy;
    public PricingService(IPricingStrategy strategy) => _strategy = strategy;
    public decimal GetPrice(Order o) => _strategy.Price(o);
}

public sealed record Order(decimal Base);
