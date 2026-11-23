using Microsoft.Extensions.DependencyInjection;
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
        private bool _isLoggingIn;
        private readonly IServiceProvider _serviceProvider;

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

        public LogInViewModel(Window window, IServiceProvider serviceProvider)
        {
            _window = window;
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, () => !_isLoggingIn);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            _serviceProvider = serviceProvider;
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                _isLoggingIn = true;
                ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();

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
                    result = await authService.LogInAsync(UsernameOrEmail, Password);
                }

                if (!result.IsSuccessed)
                {
                    SetError(nameof(Password), "Username or password is incorrect!");
                    return;
                }

                if (!SetCurrentUser(result))
                    return;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                    _window.Close();
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Login Error", "An error occurred during login");
                // Consider logging the actual exception
            }
            finally
            {
                _isLoggingIn = false;
                ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
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
            _ = ShowWarningAsync("Validation Error", errorMessage);
        }

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var error) ? error : string.Empty;

        private bool SetCurrentUser(IdentityResultViewModel result)
        {
            if (string.IsNullOrWhiteSpace(result.UserName))
            {
                _ = ShowWarningAsync("User Error", "Unable to set user!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(result.Role))
            {
                _ = ShowWarningAsync("Role Error", "Invalid user role!");
                return false;
            }

            UserSession.Username = result.UserName;
            UserSession.Role = result.Role;
            return true;
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }

        private async Task ShowWarningAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new WarningDialog($"{title}\t{message}");
            });
        }
    }
}