using ProjectManager.Services;
using ProjectManager.StartUp;
using System.Windows;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ExceptionHandle();

            await StartUp.SplashScreen.Start();

            await AutoMigration.MigrateAsync();

            await Seed.SeedRolesAsync();

            await Seed.SeedAdminAsync();

        }
        private void ExceptionHandle()
        {
            // Handle UI Thread Exceptions
            this.DispatcherUnhandledException += ExceptionHandler.App_DispatcherUnhandledException;

            // Handle Non-UI Thread Exceptions
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.CurrentDomain_UnhandledException;

            // Handle Task Thread Exceptions
            TaskScheduler.UnobservedTaskException += ExceptionHandler.TaskScheduler_UnobservedTaskException;
        }
    }

}
