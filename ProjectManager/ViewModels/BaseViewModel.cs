using ProjectManager.Constants;
using ProjectManager.Views.Database;
using ProjectManager.Views.Dialogs;
using ProjectManager.Views.Identities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<string, string> _errors = new();

    public ICommand BackupAndRestoreCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand LogOutCommand { get; }

    public BaseViewModel()
    {
        BackupAndRestoreCommand = new AsyncRelayCommand(BackupAndRestoreAsync);
        LogOutCommand = new AsyncRelayCommand(LogOutAsync);
        CloseCommand = new RelayCommand(Close); // Keeping this synchronous
    }

    #region Command Implementations
    private async Task BackupAndRestoreAsync()
    {
        if (UserSession.Role is Roles.Admin)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new BackupWindow().ShowDialog();
            });
        }
        else
        {
            await ShowWarningDialogAsync("This action requires administrator privileges.");
        }
    }

    private void Close()
    {
        Application.Current.Shutdown();
    }

    public async Task LogOutAsync()
    {
        var result = await ShowConfirmationDialogAsync("Are you sure you want to log out?");
        if (!result) return;

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var logInWindow = new LogInWindow();

            foreach (Window window in Application.Current.Windows)
            {
                if (window != logInWindow)
                    window.Close();
            }

            UserSession.ClearSession();
            logInWindow.Show();
        });

        await ShowInformationDialogAsync("Successfully logged out!");
    }
    #endregion

    #region Property Handling
    protected bool SetProperty<T>(ref T field, T value, bool validate = false, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        if (validate) ValidateProperty(propertyName, value);
        return true;
    }

    private void ValidateProperty(string propertyName, object value)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this) { MemberName = propertyName };

        if (!Validator.TryValidateProperty(value, validationContext, validationResults))
        {
            _errors[propertyName] = string.Join("\n", validationResults.Select(vr => vr.ErrorMessage));
        }
        else
        {
            _errors.Remove(propertyName);
        }
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Async Dialog Helpers
    private async Task ShowInformationDialogAsync(string message)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            new InformationDialog(message).ShowDialog();
        });
    }

    private async Task ShowWarningDialogAsync(string message)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            new WarningDialog(message).ShowDialog();
        });
    }

    private async Task<bool> ShowConfirmationDialogAsync(string message)
    {
        return await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new ConfirmationDialog(message);
            dialog.ShowDialog();
            return dialog.IsConfirmed;
        });
    }
    #endregion

    #region Async Command Implementation
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    await _execute().ConfigureAwait(false);
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
    #endregion
}