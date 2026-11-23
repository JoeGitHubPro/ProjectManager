using ProjectManager.Views.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ProjectManager.Services
{
    public static class ExceptionHandler
    {
        private static string _lastKnownState = "Application Started";
        private const string LogFileName = "Log.txt";
        private static readonly object _logLock = new object();

        public static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleExceptionAsync(e.Exception, "UI Thread Error", isCritical: false).Wait();
            e.Handled = true; // Mark as handled to prevent app crash
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                // Run synchronously since we might be crashing
                HandleExceptionAsync(ex, "Domain-Wide Error", isCritical: true).Wait();
            }
        }

        public static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleExceptionAsync(e.Exception, "Unobserved Task Error", isCritical: false).Wait();
            e.SetObserved();
        }

        public static async Task HandleExceptionAsync(Exception ex, string errorType, bool isCritical)
        {
            try
            {
                var logEntry = CreateLogEntry(ex, errorType, isCritical);

                // Async file writing
                await WriteLogAsync(logEntry).ConfigureAwait(false);

                // Show error dialog on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    new ErrorDialog(
                        $"{(isCritical ? "CRITICAL: " : "")}{errorType}",
                        $"{ex.Message}\n\nLast State: {_lastKnownState}")
                    .ShowDialog();
                });
            }
            catch (Exception loggingEx)
            {
                // Fallback to console if logging fails
                new ErrorDialog("Failed to log error: ", $"{loggingEx}");
                new ErrorDialog($"Original error: ", $"{ex}");
            }
        }

        private static string CreateLogEntry(Exception ex, string errorType, bool isCritical)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {(isCritical ? "CRITICAL " : "")}{errorType}\n" +
                   $"Last State: {_lastKnownState}\n" +
                   $"Message: {ex.Message}\n" +
                   $"Type: {ex.GetType().FullName}\n" +
                   $"Stack Trace:\n{ex.StackTrace}\n" +
                   $"Inner Exception: {ex.InnerException?.ToString() ?? "None"}\n\n";
        }

        private static async Task WriteLogAsync(string logEntry)
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_logLock)
                    {
                        File.AppendAllText(LogFileName, logEntry);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                new ErrorDialog($"Failed to write to log file: ", $"{ex.Message}");
            }
        }

        public static void SaveState(string newState)
        {
            _lastKnownState = newState ?? throw new ArgumentNullException(nameof(newState));
        }

        public static async Task LogWarningAsync(string message)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] WARNING\n" +
                          $"Last State: {_lastKnownState}\n" +
                          $"Message: {message}\n\n";

            await WriteLogAsync(logEntry).ConfigureAwait(false);
        }
    }
}