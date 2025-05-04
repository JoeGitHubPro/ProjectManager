using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ProjectManager.Config;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Views.Dialogs;
using System.Windows;

namespace ProjectManager.Database
{
    public class DatabaseBackupService : IAsyncDisposable, IDisposable
    {
        private bool _disposed = false;
        private readonly string _serverName;
        private readonly string _databaseName;
        private SqlConnection? _connection;

        public DatabaseBackupService()
        {
            var config = Task.Run(() => ConfigHelper.LoadConfigAsync<AppConfig>("jsconfig.json"));
            var builder = new SqlConnectionStringBuilder(config.Result.ConnectionString);
            _serverName = builder.DataSource;
            _databaseName = builder.InitialCatalog;
        }

        public async Task<bool> BackupDatabaseAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Save Database Backup",
                    Filter = "Backup Files (*.bak)|*.bak",
                    FileName = $"{_databaseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return false; // User canceled

                await using var context = new AppDbContext();
                var backupFilePath = saveFileDialog.FileName;
                var backupQuery = $"""
                    BACKUP DATABASE [{_databaseName}] 
                    TO DISK = '{backupFilePath}' 
                    WITH FORMAT, INIT, 
                    NAME = 'Backup of {_databaseName}'
                    """;

                await context.Database.ExecuteSqlRawAsync(backupQuery);
                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Backup Failed", ex);
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Backup File",
                    Filter = "Backup Files (*.bak)|*.bak"
                };

                if (openFileDialog.ShowDialog() != true)
                    return false; // User canceled

                var backupFilePath = openFileDialog.FileName;
                var masterConnectionString = $"Server={_serverName};Database=master;Trusted_Connection=True;Integrated Security=SSPI;TrustServerCertificate=True;MultipleActiveResultSets=true";

                _connection = new SqlConnection(masterConnectionString);
                await _connection.OpenAsync();

                await ExecuteSqlCommandAsync($"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                await ExecuteSqlCommandAsync($"RESTORE DATABASE [{_databaseName}] FROM DISK = '{backupFilePath}' WITH REPLACE");
                await ExecuteSqlCommandAsync($"ALTER DATABASE [{_databaseName}] SET MULTI_USER");

                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Restore Failed", ex);
                return false;
            }
        }

        private async Task ExecuteSqlCommandAsync(string commandText)
        {
            if (_connection == null) throw new InvalidOperationException("Connection not established");

            await using var command = new SqlCommand(commandText, _connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task ShowErrorDialogAsync(string title, Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, $"""
                    {ex.Message}
                    
                    Database: {_databaseName}
                    Server: {_serverName}
                    {(ex.InnerException != null ? $"Details: {ex.InnerException.Message}" : "")}
                    """);
            });
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
                _connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DatabaseBackupService()
        {
            Dispose(false);
        }
    }
}