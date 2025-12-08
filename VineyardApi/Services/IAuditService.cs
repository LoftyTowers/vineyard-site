using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IAuditService
    {
        Task<Result<List<AuditLog>>> GetRecentAsync(int count, CancellationToken cancellationToken);
    }
}
