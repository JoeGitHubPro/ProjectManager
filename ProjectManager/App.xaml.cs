using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using System.Windows;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (AppDbContext _context = new AppDbContext())
            {
                var pendingMigrations = _context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    _context.Database.Migrate();
                    MessageBox.Show("Database migrated successfully.");
                }
            }
        }
    }

}
