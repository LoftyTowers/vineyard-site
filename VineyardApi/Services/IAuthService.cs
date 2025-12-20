using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IAuthService
    {
        Task<Result<string>> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    }
}
