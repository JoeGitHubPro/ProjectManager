using ProjectManager.Views;
using ProjectManager.Views.Identities;

namespace ProjectManager.StartUp
{
    public static class SplashScreen
    {

        public static async Task Start()
        {
            // Show the splash screen
            SplashWindow splashScreen = new SplashWindow();
            splashScreen.Show();
            LogInWindow mainWindow = new LogInWindow();

            // Simulate loading process
            await Task.Run(() =>
            {
                // Simulate a delay (e.g., loading data or initializing the application)
                System.Threading.Thread.Sleep(5500); // Delay
            }).ContinueWith(t =>
            {
                // Close the splash screen and open the main window
                splashScreen.Dispatcher.Invoke(() =>
                {
                    splashScreen.Close();
                    mainWindow.Show();
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());


        }
    }
}
