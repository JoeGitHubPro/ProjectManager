using ProjectManager.Services;
using ProjectManager.StartUp;
using ProjectManager.Views.Dialogs;
using System.Windows;

namespace ProjectManager
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Setup global exception handling first
                RegisterExceptionHandlers();

                // Show splash screen immediately
                await StartUp.SplashScreen.StartAsync();

                // Run initialization tasks sequentially
                await InitializeApplicationAsync();
            }
            catch (Exception ex)
            {
                HandleStartupFailure(ex);
                Shutdown(1); // Non-zero exit code indicates error
            }
        }

        private async Task InitializeApplicationAsync()
        {
            // Order matters - migrate first, then seed
            await AutoMigration.MigrateAsync();
            await Seed.SeedRolesAsync();
            await Seed.SeedAdminAsync();
        }

        private void RegisterExceptionHandlers()
        {
            DispatcherUnhandledException += ExceptionHandler.App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += ExceptionHandler.TaskScheduler_UnobservedTaskException;
        }

        private void HandleStartupFailure(Exception ex)
        {
            // Ensure we're on the UI thread for message boxes
            Current.Dispatcher.Invoke(() =>
            {
                new ErrorDialog("Fatal Error", $"Application failed to start: {ex.Message}");
            });
        }
    }
}