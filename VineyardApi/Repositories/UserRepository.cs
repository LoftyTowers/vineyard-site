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

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
