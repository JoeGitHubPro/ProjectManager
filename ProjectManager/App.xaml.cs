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

            await StartUp.SplashScreen.Start();

            await AutoMigration.MigrateAsync();

            await Seed.SeedRolesAsync();

            await Seed.SeedAdminAsync();

        }
    }

}
