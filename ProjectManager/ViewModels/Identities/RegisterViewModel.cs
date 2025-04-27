using Microsoft.AspNetCore.Identity;
using ProjectManager.Service;
using ProjectManager.Validators;
using ProjectManager.Views.Identities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly Window _window;

        private string _username;
        private string _email;
        private string _phoneNumber;
        private string _password;
        private string _confirmPassword;
        private string _selectedRole;
        private bool _hasError;

        private string _usernameError;
        private string _emailError;
        private string _phoneError;
        private string _passwordError;
        private string _confirmPasswordError;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        public string UsernameError
        {
            get => _usernameError;
            set { _usernameError = value; OnPropertyChanged(); }
        }

        public string EmailError
        {
            get => _emailError;
            set { _emailError = value; OnPropertyChanged(); }
        }

        public string PhoneError
        {
            get => _phoneError;
            set { _phoneError = value; OnPropertyChanged(); }
        }

        public string PasswordError
        {
            get => _passwordError;
            set { _passwordError = value; OnPropertyChanged(); }
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set { _confirmPasswordError = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles { get; }
        public ICommand RegisterCommand { get; }
        public ICommand OpenLoginCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(Window window)
        {
            _window = window;

            Roles = new ObservableCollection<string>{
            Constants.Roles.Admin,
            Constants. Roles.Manager,
            Constants. Roles.User
            };

            SelectedRole = Roles.FirstOrDefault();
            RegisterCommand = new RelayCommand(RegisterUser);
            OpenLoginCommand = new RelayCommand(OpenLogin);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Cancel() => _window.Close();

        private void RegisterUser()
        {
            ResetErrors();

            if (!ValidateInputs())
                return;

            using (var service = new AuthenticationService())
            {
                // Pass the selected role name directly, not using Enum.Parse
                IdentityResult result = Task.Run(() => service.RegisterAsync(Username, Email, PhoneNumber, Password, SelectedRole)).Result;

                if (result.Succeeded)
                {
                    MessageBox.Show("Registration successful!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Errors.FirstOrDefault()?.Description ?? "Registration failed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ResetObjStatus();
                }
            }
        }


        private void ResetObjStatus()
        {
            _email = Email = string.Empty;
            _username = Username = string.Empty;
        }

        private void OpenLogin()
        {
            new LogInWindow().Show();
            _window.Close();
        }

        private bool ValidateInputs()
        {
            HasError = false;

            if (string.IsNullOrWhiteSpace(Username) || !StringValidator.ContainsOnlyLettersAndDigits(Username))
            {
                UsernameError = "Invalid username!";
                HasError = true;
            }

            if (string.IsNullOrWhiteSpace(Email) || !StringValidator.IsValidEmail(Email))
            {
                EmailError = "Invalid email address!";
                HasError = true;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                PhoneError = "Phone number is required!";
                HasError = true;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required!";
                HasError = true;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword) || Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match!";
                HasError = true;
            }

            return !HasError;
        }

        private void ResetErrors()
        {
            UsernameError = EmailError = PhoneError = PasswordError = ConfirmPasswordError = string.Empty;
        }
    }
}
