using ProjectManager.Constants;
using ProjectManager.Service;
using ProjectManager.Views;
using ProjectManager.Views.Dialogs;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class LogInViewModel : BaseViewModel
    {
        private readonly Window _window;
        private readonly Dictionary<string, string> _errors = new();

        private string _usernameOrEmail;
        public string UsernameOrEmail
        {
            get => _usernameOrEmail;
            set => SetProperty(ref _usernameOrEmail, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isPasswordVisible = true;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public string UsernameError => GetError(nameof(UsernameOrEmail));
        public string PasswordError => GetError(nameof(Password));

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        public LogInViewModel(Window window)
        {
            _window = window;
            LoginCommand = new RelayCommand(ExecuteLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ExecuteLogin()
        {
            ClearErrors();
            ValidateFields();

            if (_errors.Any())
            {
                OnPropertyChanged(nameof(UsernameError));
                OnPropertyChanged(nameof(PasswordError));
                return;
            }

            IdentityResultViewModel result;

            using (var authService = new AuthenticationService())
            {
                result = Task.Run(() => authService.LogInAsync(UsernameOrEmail, Password)).Result;
            }

            if (!result.IsSuccessed)
            {
                SetError(nameof(Password), "Username or password is incorrect!");
                return;
            }

            if (!SetCurrentUser(result))
                return;

            new MainWindow().Show();
            _window.Close();
        }

        private void ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(UsernameOrEmail))
                _errors[nameof(UsernameOrEmail)] = "Username or email is required!";

            if (string.IsNullOrWhiteSpace(Password))
                _errors[nameof(Password)] = "Password is required!";
        }

        private void ClearErrors()
        {
            _errors.Clear();
            OnPropertyChanged(nameof(UsernameError));
            OnPropertyChanged(nameof(PasswordError));
        }

        private void SetError(string propertyName, string errorMessage)
        {
            _errors[propertyName] = errorMessage;
            OnPropertyChanged(propertyName + "Error");
            new WarningDialog(errorMessage);
        }

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var error) ? error : string.Empty;

        private bool SetCurrentUser(IdentityResultViewModel result)
        {
            if (string.IsNullOrWhiteSpace(result.UserName))
            {
                new WarningDialog("Unable to set user!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(result.Role))
            {
                new WarningDialog("Invalid user role!");
                return false;
            }

            UserSession.Username = result.UserName;
            UserSession.Role = result.Role;
            return true;
        }
    }
}
