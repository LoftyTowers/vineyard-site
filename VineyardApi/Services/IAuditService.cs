using VineyardApi.Models;
using VineyardApi.Infrastructure;

namespace VineyardApi.Services
{
    public interface IAuditService
    {
        Task<Result<List<AuditLog>>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default);
    }
}
