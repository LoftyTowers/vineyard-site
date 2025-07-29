using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        public AuditService(IAuditRepository repository)
        {
            _repository = repository;
        }

        public Task<List<AuditLog>> GetRecentAsync(int count = 100)
        {
            return _repository.GetRecentAsync(count);
        }
    }
}
