using ProjectManager.Constants;
using ProjectManager.ViewModels;
using ProjectManager.Views.Dialogs;
using ProjectManager.Views.Identities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

public class BaseViewModel : INotifyPropertyChanged//, IDataErrorInfo
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<string, string> _errors = new();

    public ICommand CloseCommand { get; }
    public ICommand LogOutCommand { get; }

    public BaseViewModel()
    {
        LogOutCommand = new RelayCommand(LogOut);
        CloseCommand = new RelayCommand(Close);

    }
    private void Close()
    {
        Application.Current.Shutdown();
    }
    public void LogOut()
    {
        // Confirm log out
        var result = new ConfirmationDialog("Are you sure you want to log out?");
        result.ShowDialog();

        if (!result.IsConfirmed)
            return;

        LogInWindow logInWindow = new LogInWindow();

        foreach (Window window in Application.Current.Windows)
        {
            if (window != logInWindow)
                window.Close();
        }

        UserSession.Username = null;
        UserSession.Email = null;
        UserSession.Role = null;

        logInWindow.Show();

        // Inform user of successful log out
        new InformationDialog("Successfully logged out!");
    }
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

}