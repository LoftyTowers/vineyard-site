using Microsoft.EntityFrameworkCore;

namespace VineyardApi.Data
{
    public static class DatabaseExtensions
    {
        public static void MigrateDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VineyardDbContext>();
            db.Database.Migrate();
        }
    }
}
