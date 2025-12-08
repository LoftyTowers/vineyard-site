public interface IProductCatalog { Task<Product?> GetAsync(string sku, CancellationToken ct); }

public sealed class CachingCatalog : IProductCatalog
{
    private readonly IProductCatalog _inner;
    private readonly IMemoryCache _cache;

    public CachingCatalog(IProductCatalog inner, IMemoryCache cache) { _inner = inner; _cache = cache; }

    public Task<Product?> GetAsync(string sku, CancellationToken ct) =>
        _cache.GetOrCreateAsync(sku, _ => _inner.GetAsync(sku, ct));
}

public sealed record Product(string Sku, string Name);
