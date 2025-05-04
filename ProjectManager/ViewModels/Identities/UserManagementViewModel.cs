using ProjectManager.Service;
using ProjectManager.Validators;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
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
        private bool _isLoading;

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
            set { _searchText = value; OnPropertyChanged(); _ = FilterUsersAsync(); }
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
            EditUserCommand = new AsyncRelayCommand(EditUserAsync);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync);
            SaveUserCommand = new AsyncRelayCommand(SaveUserAsync, () => !_isLoading);
            CancelFormCommand = new RelayCommand(CancelForm);
            RefreshCommand = new AsyncRelayCommand(LoadUsersAsync);

            _ = LoadUsersAsync(); // Fire-and-forget async initialization
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                _isLoading = true;
                ((AsyncRelayCommand)RefreshCommand)?.RaiseCanExecuteChanged();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Users.Clear();
                    IsFormVisible = false;
                });

                using var service = new AuthenticationService();
                var users = await service.GetAllUsersAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                    StatusMessage = $"Loaded {Users.Count} users";
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading users", ex.Message);
            }
            finally
            {
                _isLoading = false;
                ((AsyncRelayCommand)RefreshCommand)?.RaiseCanExecuteChanged();
            }
        }

        private async Task FilterUsersAsync()
        {
            try
            {
                _isLoading = true;
                await Application.Current.Dispatcher.InvokeAsync(() => Users.Clear());

                using var service = new AuthenticationService();
                var allUsers = await service.GetAllUsersAsync();

                var filtered = string.IsNullOrWhiteSpace(SearchText)
                    ? allUsers
                    : allUsers.Where(u =>
                        (u.UserName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.PhoneNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var user in filtered)
                    {
                        Users.Add(user);
                    }
                    StatusMessage = $"Found {Users.Count} users";
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error filtering users", ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ShowAddUserForm()
        {
            CurrentUser = new UserViewModel();
            FormTitle = "Add New User";
            IsFormVisible = true;
        }

        private async Task EditUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                _isLoading = true;
                ((AsyncRelayCommand)EditUserCommand)?.RaiseCanExecuteChanged();

                using var service = new AuthenticationService();
                var user = await service.GetUserAsync(SelectedUser.UserName);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
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
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading user for edit", ex.Message);
            }
            finally
            {
                _isLoading = false;
                ((AsyncRelayCommand)EditUserCommand)?.RaiseCanExecuteChanged();
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            var confirmed = await ShowConfirmationDialogAsync(
                "Confirm Delete",
                $"Are you sure you want to delete '{SelectedUser.UserName}'?");

            if (!confirmed) return;

            try
            {
                _isLoading = true;
                ((AsyncRelayCommand)DeleteUserCommand)?.RaiseCanExecuteChanged();

                using var service = new AuthenticationService();
                var result = await service.DeleteUserAsync(SelectedUser.Email);

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (result.IsSuccessed)
                    {
                        StatusMessage = $"User '{SelectedUser.UserName}' deleted";
                        await LoadUsersAsync();
                    }
                    else
                    {
                        await ShowErrorAsync("Delete Error", $"Delete failed: {result.Errors.Description}");
                    }
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error deleting user", ex.Message);
            }
            finally
            {
                _isLoading = false;
                ((AsyncRelayCommand)DeleteUserCommand)?.RaiseCanExecuteChanged();
            }
        }

        private async Task SaveUserAsync()
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
                _isLoading = true;
                ((AsyncRelayCommand)SaveUserCommand)?.RaiseCanExecuteChanged();

                using var service = new AuthenticationService();
                bool success;

                if (string.IsNullOrEmpty(CurrentUser.UserId))
                {
                    var result = await service.RegisterAsync(
                        CurrentUser.UserName,
                        CurrentUser.Email,
                        CurrentUser.PhoneNumber,
                        CurrentUser.Password,
                        CurrentUser.Roles);

                    success = result.Succeeded;
                }
                else
                {
                    success = await service.EditUserAsync(CurrentUser);
                }

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (success)
                    {
                        StatusMessage = "User saved successfully";
                        await ShowInfoAsync("Success", StatusMessage);
                        await LoadUsersAsync();
                        IsFormVisible = false;
                    }
                    else
                    {
                        StatusMessage = "Failed to save user";
                        await ShowErrorAsync("Save Error", "Operation failed. Check the input data.");
                    }
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving user", ex.Message);
            }
            finally
            {
                _isLoading = false;
                ((AsyncRelayCommand)SaveUserCommand)?.RaiseCanExecuteChanged();
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

        #region Dialog Helpers
        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }

        private async Task ShowInfoAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new InformationDialog($"{title}\t{message}");
            });
        }

        private async Task<bool> ShowConfirmationDialogAsync(string title, string message)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog($"{title}\t{message}");
                dialog.ShowDialog();
                return dialog.IsConfirmed;
            });
        }
        #endregion
    }
}