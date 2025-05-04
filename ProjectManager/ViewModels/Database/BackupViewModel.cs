using ProjectManager.Database;
using ProjectManager.Views.Database;
using ProjectManager.Views.Dialogs;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Database
{
    public class BackupViewModel : BaseViewModel
    {
        private bool _isOperating;
        private string _operationStatus = string.Empty;

        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand CloseCommand { get; }

        public bool IsOperating
        {
            get => _isOperating;
            set => SetProperty(ref _isOperating, value);
        }

        public string OperationStatus
        {
            get => _operationStatus;
            set => SetProperty(ref _operationStatus, value);
        }

        public BackupViewModel()
        {
            BackupDatabaseCommand = new AsyncRelayCommand(BackupDatabaseAsync);
            RestoreDatabaseCommand = new AsyncRelayCommand(RestoreDatabaseAsync);
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            Application.Current.Windows.OfType<BackupWindow>().FirstOrDefault()?.Close();
        }

        public async Task BackupDatabaseAsync()
        {
            IsOperating = true;
            OperationStatus = "Creating database backup...";

            try
            {
                await using var backupService = new DatabaseBackupService();
                var success = await backupService.BackupDatabaseAsync();

                await ShowResultDialogAsync(
                    success,
                    "Database backup completed successfully!",
                    "Backup failed!");
            }
            finally
            {
                IsOperating = false;
                OperationStatus = string.Empty;
            }
        }

        public async Task RestoreDatabaseAsync()
        {
            IsOperating = true;
            OperationStatus = "Restoring database...";

            try
            {
                await using var backupService = new DatabaseBackupService();
                var success = await backupService.RestoreDatabaseAsync();

                await ShowResultDialogAsync(
                    success,
                    "Database restored successfully!",
                    "Restore failed!");
            }
            finally
            {
                IsOperating = false;
                OperationStatus = string.Empty;
            }
        }

        private async Task ShowResultDialogAsync(bool success, string successMessage, string errorMessage)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (success)
                {
                    new InformationDialog(successMessage).ShowDialog();
                }
                else
                {
                    new WarningDialog(errorMessage).ShowDialog();
                }
            });
        }
    }
}