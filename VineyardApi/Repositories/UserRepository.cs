using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly VineyardDbContext _context;
        public UserRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            return _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
    }
}
