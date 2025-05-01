using ProjectManager.Views.Dialogs;
using System.IO;

namespace ProjectManager.Services
{
    public static class ExceptionHandler
    {
        private static string _lastKnownState = "Default State"; // Store last application state

        public static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception, "An unexpected error occurred:", "Error");

            // Show error dialog
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                new ErrorDialog("An error occurred", e.Exception.Message);
            });

            // Prevent application from crashing
            e.Handled = true;
        }


        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex, "A critical error occurred", "Critical Error");
            }
        }

        public static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception, "An unobserved task error occurred", "Task Error");
            e.SetObserved(); // Prevents application crash
        }

        public static void LogException(Exception ex, string info, string type)
        {
            string logMessage = $"{DateTime.Now}\t=> Info: {info}\t Error Type: {type}\n" +
                                $"Message: {ex.Message}\n" +
                                $"Inner Exception: {ex.InnerException?.Message ?? "None"}\n" +
                                $"Stack Trace:\n{ex.StackTrace}\n\n";

            File.AppendAllText("Log.txt", logMessage);

            // Show error dialog (ensure it's shown on UI thread)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                new ErrorDialog(info, $"{type}: {ex.Message}");
            });
        }

        public static void SaveState(string newState)
        {
            _lastKnownState = newState;
        }
    }
}
