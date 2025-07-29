using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IAuditService
    {
        Task<List<AuditLog>> GetRecentAsync(int count = 100);
    }
}
