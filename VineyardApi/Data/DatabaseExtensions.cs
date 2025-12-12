using Microsoft.EntityFrameworkCore;
using System;

namespace VineyardApi.Data
{
    public static class DatabaseExtensions
    {
        public static void MigrateDatabase(this WebApplication app)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<VineyardDbContext>();
                db.Database.Migrate();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
