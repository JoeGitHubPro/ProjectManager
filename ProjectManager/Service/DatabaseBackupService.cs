using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ProjectManager.Config;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Views.Dialogs;

namespace ProjectManager.Database
{
    public class DatabaseBackupService : IDisposable
    {
        private bool _disposed = false;
        private string _serverName;
        private string _databaseName;
        public DatabaseBackupService()
        {
            var config = ConfigHelper.LoadConfig<AppConfig>("jsconfig.json");

            var builder = new SqlConnectionStringBuilder(config.ConnectionString);

            _serverName = builder.DataSource;
            _databaseName = builder.InitialCatalog;
        }
        /// <summary>
        /// Backup the database and let the user choose the save location.
        /// </summary>
        public bool BackupDatabase()
        {
            using (AppDbContext context = new AppDbContext())
            {
                try
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Title = "Save Database Backup",
                        Filter = "Backup Files (*.bak)|*.bak",
                        FileName = $"{_databaseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string backupFilePath = saveFileDialog.FileName;
                        string backupQuery = $"BACKUP DATABASE [{_databaseName}] TO DISK = '{backupFilePath}' WITH FORMAT, INIT, NAME = 'Backup of {_databaseName}'";

                        context.Database.ExecuteSqlRaw(backupQuery);
                        return true;
                    }
                    return false; // User canceled
                }
                catch (Exception ex)
                {
                    new ErrorDialog(ex.Message, ex.InnerException?.Message);
                    return false;
                }
            }

        }

        /// <summary>
        /// Restore the database from a user-selected backup file.
        /// </summary>
        public bool RestoreDatabase()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Backup File",
                    Filter = "Backup Files (*.bak)|*.bak"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string backupFilePath = openFileDialog.FileName;
                    string connectionString = $"Server={_serverName};Database=master;Trusted_Connection=True;Integrated Security=SSPI;TrustServerCertificate=True;MultipleActiveResultSets=true";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        ExecuteSqlCommand(connection, $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
                        ExecuteSqlCommand(connection, $"RESTORE DATABASE [{_databaseName}] FROM DISK = '{backupFilePath}' WITH REPLACE;");
                        ExecuteSqlCommand(connection, $"ALTER DATABASE [{_databaseName}] SET MULTI_USER;");
                    }
                    return true;
                }
                return false; // User canceled
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ExecuteSqlCommand(SqlConnection connection, string commandText)
        {
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        ~DatabaseBackupService()
        {
            Dispose(false);
        }
    }


}
