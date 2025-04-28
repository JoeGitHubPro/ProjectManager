using ProjectManager.Service;
using ProjectManager.Validators;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly Window _window;
        private bool _isFormVisible;
        private string _formTitle;
        private string _statusMessage;
        private UserViewModel _currentUser;
        private string _confirmPassword;

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
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterUsers(); // Call a method to filter your Users list
            }
        }


        // Error properties
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
        public ICommand CloseCommand { get; }

        public UserManagementViewModel()
        {
            // Initialize commands
            ShowAddUserFormCommand = new RelayCommand(ShowAddUserForm);
            EditUserCommand = new RelayCommand(EditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            SaveUserCommand = new RelayCommand(SaveUser);
            CancelFormCommand = new RelayCommand(CancelForm);
            RefreshCommand = new RelayCommand(LoadUsers);
            CloseCommand = new RelayCommand(CloseWindow);

            LoadUsers();
        }

        private void FilterUsers()
        {
            try
            {
                Users.Clear();
                using var service = new AuthenticationService();

                var users = Task.Run(() => service.GetAllUsersAsync()).Result;

                IEnumerable<UserViewModel> filteredUsers = users;

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredUsers = users.Where(u =>
                        (u.UserName != null && u.UserName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Email != null && u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var user in filteredUsers)
                {
                    Users.Add(user);
                }

                StatusMessage = $"Found {Users.Count} users";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error filtering users";
                MessageBox.Show($"Error filtering users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                Users.Clear();
                using var service = new AuthenticationService();
                var users = Task.Run(() => service.GetAllUsersAsync()).Result;
                foreach (var user in users)
                {
                    Users.Add(user);
                }
                StatusMessage = $"Loaded {Users.Count} users";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error loading users";
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

            string? UserId = string.Empty;
            using (AuthenticationService service = new AuthenticationService())
            {
                UserId = service.GetUser(SelectedUser.UserName).Id;
            }
            CurrentUser = new UserViewModel
            {
                UserId = UserId,
                UserName = SelectedUser.UserName,
                Email = SelectedUser.Email,
                PhoneNumber = SelectedUser.PhoneNumber,
                Roles = SelectedUser.Roles
            };

            FormTitle = "Edit User";
            IsFormVisible = true;
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            if (MessageBox.Show($"Are you sure you want to delete user '{SelectedUser.UserName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

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
                    MessageBox.Show($"Failed to delete user: {result.Errors.Description}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveUser()
        {
            ClearErrors();

            // Validate inputs
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

            // If any errors, return
            if (!string.IsNullOrWhiteSpace(UserNameError) || !string.IsNullOrWhiteSpace(EmailError) ||
                !string.IsNullOrWhiteSpace(PhoneError) || !string.IsNullOrWhiteSpace(RoleError) ||
                !string.IsNullOrWhiteSpace(ConfirmPasswordError))
            {
                OnPropertyChanged(nameof(UserNameError));
                OnPropertyChanged(nameof(EmailError));
                OnPropertyChanged(nameof(PhoneError));
                OnPropertyChanged(nameof(RoleError));
                OnPropertyChanged(nameof(ConfirmPasswordError));
                return;
            }

            try
            {
                using var service = new AuthenticationService();
                bool success;

                if (string.IsNullOrEmpty(CurrentUser.UserId))
                {
                    // Add new user
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
                    // Update existing user
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
                    MessageBox.Show("Operation failed. Please check the details and try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelForm()
        {
            IsFormVisible = false;
            ClearErrors();
        }

        private void CloseWindow()
        {
            _window.Close();
        }

        private void ClearErrors()
        {
            UserNameError = EmailError = PhoneError = RoleError = PasswordError = ConfirmPasswordError = string.Empty;
            OnPropertyChanged(nameof(UserNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PhoneError));
            OnPropertyChanged(nameof(RoleError));
            OnPropertyChanged(nameof(PasswordError));
            OnPropertyChanged(nameof(ConfirmPasswordError));
        }
    }
}