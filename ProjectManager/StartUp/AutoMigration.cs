using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Views.Dialogs;

namespace ProjectManager.StartUp
{
    public static class AutoMigration
    {
        public static async Task MigrateAsync()
        {
            try
            {
                await using (var context = new AppDbContext())
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync()
                        .ConfigureAwait(false);

                    if (pendingMigrations.Any())
                    {
                        await context.Database.MigrateAsync()
                            .ConfigureAwait(false);

                        await ShowMigrationSuccessDialogAsync()
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleMigrationErrorAsync(ex)
                    .ConfigureAwait(false);
                throw; // Re-throw to let caller handle if needed
            }
        }

        private static async Task ShowMigrationSuccessDialogAsync()
        {
            var dialog = new InformationDialog("Database migrated successfully.");
            // If your dialog has async methods, use them here
            // Example: await dialog.ShowAsync();
        }

        private static async Task HandleMigrationErrorAsync(Exception ex)
        {
            // Implement proper error logging/notification here
            // Example: await Logger.LogErrorAsync(ex);
            Console.Error.WriteLine($"Migration failed: {ex.Message}");
        }
    }
}