using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Views.Dialogs;

namespace ProjectManager.StartUp
{
    public class AutoMigration
    {
        public async static Task MigrateAsync()
        {
            using (AppDbContext _context = new AppDbContext())
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                    new InformationDialog("Database migrated successfully.");
                }
            }
        }
    }
}
