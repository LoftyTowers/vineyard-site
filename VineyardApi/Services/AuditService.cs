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

        public async Task<Result<List<AuditLog>>> GetRecentAsync(int count, CancellationToken cancellationToken)
        {
            var logs = await _repository.GetRecentAsync(count, cancellationToken);
            return Result<List<AuditLog>>.Ok(logs);
        }
    }
}
