using ProjectManager.Views;
using ProjectManager.Views.Dialogs;
using ProjectManager.Views.Identities;
using System.Windows;

namespace ProjectManager.StartUp
{
    public static class SplashScreen
    {
        public static async Task StartAsync(IServiceProvider serviceProvider)
        {
            var splashScreen = new SplashWindow();
            var loginScreen = new LogInWindow(serviceProvider);
            splashScreen.Show();

            try
            {
                // Simulate async loading process
                await SimulateLoadingAsync().ConfigureAwait(false);

                // Switch back to UI thread before window operations
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    loginScreen.Show();
                    splashScreen.Close();
                });
            }
            catch (Exception ex)
            {
                // Handle any errors during splash screen process
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    splashScreen.Close();
                    new ErrorDialog("Error", $"Application initialization failed: {ex.Message}");
                });
                throw; // Re-throw if you want the caller to handle it
            }
        }

        private static async Task SimulateLoadingAsync()
        {
            // Replace with actual initialization logic
            await Task.Delay(6000).ConfigureAwait(false); // Better than Thread.Sleep
        }
    }
}