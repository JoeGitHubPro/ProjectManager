using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using System.Windows;

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
                    MessageBox.Show("Database migrated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
