using ProjectManager.Service;
using ProjectManager.Validators;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class EditUserViewModel : BaseViewModel
    {
        private string _userName;
        private string _oldUserName;
        private string _email;
        private string _phoneNumber;
        private string _selectedRole;
        private string _newPassword;
        private string _confirmNewPassword;

        private readonly Dictionary<string, string> _errors = new();

        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string>
{
   Constants. Roles.Admin,
  Constants.  Roles.Manager,
    Constants.Roles.User
};

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(nameof(UserName)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(nameof(SelectedRole)); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set { _confirmNewPassword = value; OnPropertyChanged(nameof(ConfirmNewPassword)); }
        }

        public string UserNameError => GetError(nameof(UserName));
        public string EmailError => GetError(nameof(Email));
        public string PhoneNumberError => GetError(nameof(PhoneNumber));
        public string RoleError => GetError(nameof(SelectedRole));
        public string NewPasswordError => GetError(nameof(NewPassword));
        public string ConfirmNewPasswordError => GetError(nameof(ConfirmNewPassword));

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Window _window;

        public EditUserViewModel(UserViewModel user, Window window)
        {
            _window = window;

            UserName = _oldUserName = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            SelectedRole = user.Roles;

            SaveCommand = new RelayCommand(SaveUser);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveUser()
        {
            ClearErrors();
            ValidateFields();

            if (_errors.Any())
            {
                OnPropertyChanged(nameof(UserNameError));
                OnPropertyChanged(nameof(EmailError));
                OnPropertyChanged(nameof(PhoneNumberError));
                OnPropertyChanged(nameof(RoleError));
                OnPropertyChanged(nameof(NewPasswordError));
                OnPropertyChanged(nameof(ConfirmNewPasswordError));
                return;
            }

            using (var service = new AuthenticationService())
            {
                var userId = service.GetUserId(_oldUserName);

                if (userId == null)
                {
                    MessageBox.Show("404\nUser not found.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedUser = new UserViewModel
                {
                    UserId = userId,
                    UserName = UserName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Roles = SelectedRole,
                    Password = NewPassword // Include the new password if provided
                };

                try
                {
                    if (Task.Run(() => service.EditUserAsync(updatedUser)).Result)
                    {
                        MessageBox.Show("User updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        _window.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update user.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update user.\n{ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Cancel() => _window.Close();

        private void ClearErrors()
        {
            _errors.Clear();
            OnPropertyChanged(nameof(UserNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PhoneNumberError));
            OnPropertyChanged(nameof(RoleError));
            OnPropertyChanged(nameof(NewPasswordError));
            OnPropertyChanged(nameof(ConfirmNewPasswordError));
        }

        private void ValidateFields()
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(UserName) || !StringValidator.ContainsOnlyLettersAndDigits(UserName))
                _errors[nameof(UserName)] = "Invalid username!";

            // Validate email
            if (string.IsNullOrWhiteSpace(Email) || !StringValidator.IsValidEmail(Email))
                _errors[nameof(Email)] = "Please enter a valid email address!";

            // Validate phone number
            if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 11)
                _errors[nameof(PhoneNumber)] = "Please enter a valid phone number!";

            // Validate role
            if (string.IsNullOrWhiteSpace(SelectedRole))
                _errors[nameof(SelectedRole)] = "Please select a role!";

            // Validate new password fields only if a new password is provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword != ConfirmNewPassword)
                    _errors[nameof(ConfirmNewPassword)] = "Passwords do not match!";
            }
        }

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var error) ? error : string.Empty;
    }
}