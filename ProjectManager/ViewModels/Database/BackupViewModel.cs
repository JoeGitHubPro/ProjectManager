using ProjectManager.Database;
using ProjectManager.Views.Database;
using ProjectManager.Views.Dialogs;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Database
{
    public class BackupViewModel : BaseViewModel
    {

        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand CloseCommand { get; }

        public BackupViewModel()
        {
            BackupDatabaseCommand = new RelayCommand(BackupDatabase);
            RestoreDatabaseCommand = new RelayCommand(RestoreDatabase);
            CloseCommand = new RelayCommand(Close);

        }

        private void Close()
        {
            Application.Current.Windows.OfType<BackupWindow>().FirstOrDefault()?.Close();

        }

        public void BackupDatabase()
        {
            using (DatabaseBackupService _backupService = new DatabaseBackupService())
            {
                bool success = _backupService.BackupDatabase();

                if (success)
                    new InformationDialog("Database backup completed successfully!");

                else
                    new WarningDialog("Backup failed!");
            }
        }

        public void RestoreDatabase()
        {
            using (DatabaseBackupService _backupService = new DatabaseBackupService())
            {
                bool success = _backupService.RestoreDatabase();

                if (success)
                    new InformationDialog("Database restored successfully!");

                else
                    new WarningDialog("Restore failed!");
            }
        }

    }
}
