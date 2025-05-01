using ProjectManager.Service;
using ProjectManager.Validators;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class UserManagementViewModel : BaseViewModel
    {
        private string _statusMessage;
        private string _searchText;
        private string _confirmPassword;
        private bool _isFormVisible;
        private string _formTitle;
        private UserViewModel _currentUser;

        public ObservableCollection<UserViewModel> Users { get; } = new();
        public ObservableCollection<string> Roles { get; } = new()
        {
            Constants.Roles.Admin,
            Constants.Roles.Manager,
            Constants.Roles.User
        };

        public UserViewModel SelectedUser { get; set; }

        public UserViewModel CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set { _isFormVisible = value; OnPropertyChanged(); }
        }

        public string FormTitle
        {
            get => _formTitle;
            set { _formTitle = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterUsers(); }
        }

        // Validation Errors
        public string UserNameError { get; private set; }
        public string EmailError { get; private set; }
        public string PhoneError { get; private set; }
        public string RoleError { get; private set; }
        public string PasswordError { get; private set; }
        public string ConfirmPasswordError { get; private set; }

        // Commands
        public ICommand ShowAddUserFormCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelFormCommand { get; }
        public ICommand RefreshCommand { get; }

        public UserManagementViewModel()
        {

            ShowAddUserFormCommand = new RelayCommand(ShowAddUserForm);
            EditUserCommand = new RelayCommand(EditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            SaveUserCommand = new RelayCommand(SaveUser);
            CancelFormCommand = new RelayCommand(CancelForm);
            RefreshCommand = new RelayCommand(LoadUsers);

            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Users.Clear();
                IsFormVisible = false;

                using var service = new AuthenticationService();
                var users = Task.Run(() => service.GetAllUsersAsync()).Result;

                foreach (var user in users)
                    Users.Add(user);

                StatusMessage = $"Loaded {Users.Count} users";
            }
            catch (Exception ex)
            {
                ShowError("loading users", ex);
            }
        }

        private void FilterUsers()
        {
            try
            {
                Users.Clear();

                using var service = new AuthenticationService();
                var allUsers = Task.Run(() => service.GetAllUsersAsync()).Result;

                var filtered = string.IsNullOrWhiteSpace(SearchText)
                    ? allUsers
                    : allUsers.Where(u =>
                        (u.UserName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.PhoneNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

                foreach (var user in filtered)
                    Users.Add(user);

                StatusMessage = $"Found {Users.Count} users";
            }
            catch (Exception ex)
            {
                ShowError("filtering users", ex);
            }
        }

        private void ShowAddUserForm()
        {
            CurrentUser = new UserViewModel();
            FormTitle = "Add New User";
            IsFormVisible = true;
        }

        private void EditUser()
        {
            if (SelectedUser == null) return;

            try
            {
                using var service = new AuthenticationService();
                var user = service.GetUser(SelectedUser.UserName);

                CurrentUser = new UserViewModel
                {
                    UserId = user.Id,
                    UserName = SelectedUser.UserName,
                    Email = SelectedUser.Email,
                    PhoneNumber = SelectedUser.PhoneNumber,
                    Roles = SelectedUser.Roles
                };

                FormTitle = "Edit User";
                IsFormVisible = true;
            }
            catch (Exception ex)
            {
                ShowError("loading user for edit", ex);
            }
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;
            var resultDialog = new ConfirmationDialog($"Are you sure you want to delete '{SelectedUser.UserName}'?");
            resultDialog.ShowDialog();

            if (!resultDialog.IsConfirmed)
                return;

            try
            {
                using var service = new AuthenticationService();
                var result = Task.Run(() => service.DeleteUserAsync(SelectedUser.Email)).Result;

                if (result.IsSuccessed)
                {
                    StatusMessage = $"User '{SelectedUser.UserName}' deleted";
                    LoadUsers();
                }
                else
                {
                    new ErrorDialog("Error", $"Delete failed: {result.Errors.Description}");
                }
            }
            catch (Exception ex)
            {
                ShowError("deleting user", ex);
            }
        }

        private void SaveUser()
        {
            ClearErrors();

            // Validation
            if (string.IsNullOrWhiteSpace(CurrentUser.UserName))
                UserNameError = "Username is required";

            if (string.IsNullOrWhiteSpace(CurrentUser.Email) || !StringValidator.IsValidEmail(CurrentUser.Email))
                EmailError = "Valid email is required";

            if (string.IsNullOrWhiteSpace(CurrentUser.PhoneNumber))
                PhoneError = "Phone number is required";

            if (string.IsNullOrWhiteSpace(CurrentUser.Roles))
                RoleError = "Role is required";

            if (!string.IsNullOrWhiteSpace(CurrentUser.Password) && CurrentUser.Password != ConfirmPassword)
                ConfirmPasswordError = "Passwords must match";

            if (HasErrors())
            {
                NotifyErrorsChanged();
                return;
            }

            try
            {
                using var service = new AuthenticationService();
                bool success;

                if (string.IsNullOrEmpty(CurrentUser.UserId))
                {
                    var result = Task.Run(() => service.RegisterAsync(
                        CurrentUser.UserName,
                        CurrentUser.Email,
                        CurrentUser.PhoneNumber,
                        CurrentUser.Password,
                        CurrentUser.Roles)).Result;

                    success = result.Succeeded;
                    StatusMessage = success ? "User added successfully" : "Failed to add user";
                }
                else
                {
                    success = Task.Run(() => service.EditUserAsync(CurrentUser)).Result;
                    StatusMessage = success ? "User updated successfully" : "Failed to update user";
                }

                if (success)
                {
                    LoadUsers();
                    IsFormVisible = false;
                }
                else
                {
                    new ErrorDialog("Error", "Operation failed. Check the input data.");
                }
            }
            catch (Exception ex)
            {
                ShowError("saving user", ex);
            }
        }

        private void CancelForm()
        {
            IsFormVisible = false;
            ClearErrors();
        }


        private void ClearErrors()
        {
            UserNameError = EmailError = PhoneError = RoleError = PasswordError = ConfirmPasswordError = string.Empty;
            NotifyErrorsChanged();
        }

        private void NotifyErrorsChanged()
        {
            OnPropertyChanged(nameof(UserNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PhoneError));
            OnPropertyChanged(nameof(RoleError));
            OnPropertyChanged(nameof(PasswordError));
            OnPropertyChanged(nameof(ConfirmPasswordError));
        }

        private bool HasErrors() =>
            !string.IsNullOrWhiteSpace(UserNameError) ||
            !string.IsNullOrWhiteSpace(EmailError) ||
            !string.IsNullOrWhiteSpace(PhoneError) ||
            !string.IsNullOrWhiteSpace(RoleError) ||
            !string.IsNullOrWhiteSpace(ConfirmPasswordError);

        private void ShowError(string context, Exception ex)
        {
            StatusMessage = $"Error {context}";
            new ErrorDialog("Error", $"Error {context}: {ex.Message}");
        }
    }
}
