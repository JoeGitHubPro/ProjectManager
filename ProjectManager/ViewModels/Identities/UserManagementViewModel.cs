using ProjectManager.Constants;
using ProjectManager.Service;
using ProjectManager.Views.Identities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Identities
{
    public class UserManagementViewModel : BaseViewModel
    {
        private ObservableCollection<UserViewModel> _users;
        private UserViewModel _selectedUser;
        private Window _window; // Store window reference

        public ObservableCollection<UserViewModel> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public UserViewModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        public UserManagementViewModel(Window window)
        {
            _window = window; // Capture reference
            Users = new ObservableCollection<UserViewModel>();
            LoadUsers();

            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            RefreshCommand = new RelayCommand(LoadUsers);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void LoadUsers()
        {
            try
            {
                using (var service = new AuthenticationService())
                {
                    var users = Task.Run(() => service.GetAllUsersAsync()).Result;
                    Users = new ObservableCollection<UserViewModel>(users);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddUser()
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
            LoadUsers();
        }

        private void EditUser()
        {
            if (SelectedUser == null) return;

            new EditUserWindow(SelectedUser).ShowDialog();
            LoadUsers();
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            if (SelectedUser.UserName == UserSession.Username)
            {
                MessageBox.Show("You cannot delete the current user!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmationResult = MessageBox.Show($"Are you sure you want to delete {SelectedUser.UserName}?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmationResult == MessageBoxResult.Yes)
            {
                try
                {
                    using (var service = new AuthenticationService())
                    {
                        var response = Task.Run(() => service.DeleteUserAsync(SelectedUser.Email)).Result;

                        if (response.IsSuccessed)
                        {
                            MessageBox.Show("User deleted successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show($"Error: {response.Errors.Code}\nFailed to delete user.\n{response.Errors.Description}",
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during deletion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseWindow()
        {
            _window?.Close(); // Close using stored reference
        }
    }
}
