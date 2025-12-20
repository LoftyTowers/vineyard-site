using Microsoft.Extensions.Logging;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<List<AuditLog>>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
        {
            try
            {
                var logs = await _repository.GetRecentAsync(count, cancellationToken);
                return Result<List<AuditLog>>.Ok(logs);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Audit log retrieval cancelled with count {Count}", count);
                return Result<List<AuditLog>>.Failure(ErrorCode.Cancelled, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent audit logs with count {Count}", count);
                return Result<List<AuditLog>>.Failure(ErrorCode.Unexpected, "Failed to load audit logs");
            }
        }
    }
}
