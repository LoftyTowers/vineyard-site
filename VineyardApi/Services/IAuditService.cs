using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IAuditService
    {
        Task<Result<List<AuditLog>>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default);
    }
}
