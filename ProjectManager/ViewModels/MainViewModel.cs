using ProjectManager.Constants;
using ProjectManager.ViewModels.CapitalManagement;
using ProjectManager.ViewModels.Identities;
using ProjectManager.Views.Dialogs;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ViewModel instances
        public ConsumerViewModel ConsumerVM { get; private set; }
        public ProjectCategoryViewModel ProjectCategoryVM { get; private set; }
        public ProjectViewModel ProjectVM { get; private set; }
        public ProjectOrderViewModel ProjectOrderVM { get; private set; }
        public UserManagementViewModel UserManagementVM { get; private set; }
        public BusinessBalanceViewModel BusinessBalanceVM { get; private set; }
        public LocationViewModel LocationVM { get; private set; }
        public WelcomeViewModel WelcomeVM { get; private set; }

        // Role-based access properties
        public bool IsProjectOrderEnabled { get; private set; }
        public bool IsProjectCategoriesEnabled { get; private set; }
        public bool IsProjectsEnabled { get; private set; }
        public bool IsConsumersEnabled { get; private set; } = true; // Enabled for all roles
        public bool IsUsersManagementEnabled { get; private set; }
        public bool IsLocationsEnabled { get; private set; }
        public bool IsBusinessBalanceEnabled { get; private set; }

        public MainViewModel()
        {
            // Initialize ViewModels (actual async loading will happen when needed)
            ConsumerVM = new ConsumerViewModel();
            ProjectCategoryVM = new ProjectCategoryViewModel();
            ProjectVM = new ProjectViewModel();
            ProjectOrderVM = new ProjectOrderViewModel();
            UserManagementVM = new UserManagementViewModel();
            BusinessBalanceVM = BusinessBalanceViewModel.Instance;
            LocationVM = new LocationViewModel();
            WelcomeVM = new WelcomeViewModel();

            SetupRoleBasedAccess();
        }

        private void SetupRoleBasedAccess()
        {
            switch (UserSession.Role)
            {
                case Roles.Admin:
                    IsUsersManagementEnabled = true;
                    IsProjectOrderEnabled = true;
                    IsProjectCategoriesEnabled = true;
                    IsProjectsEnabled = true;
                    IsBusinessBalanceEnabled = true;
                    IsLocationsEnabled = true;
                    break;

                case Roles.Manager:
                    IsProjectOrderEnabled = true;
                    IsProjectCategoriesEnabled = true;
                    IsProjectsEnabled = true;
                    IsLocationsEnabled = true;
                    IsBusinessBalanceEnabled = true;
                    break;

                // Add other roles as needed
                default:
                    // Basic access for other roles
                    IsProjectOrderEnabled = false;
                    IsProjectCategoriesEnabled = false;
                    IsProjectsEnabled = false;
                    IsBusinessBalanceEnabled = false;
                    break;
            }

            // Notify property changes for all access properties
            OnPropertyChanged(nameof(IsProjectOrderEnabled));
            OnPropertyChanged(nameof(IsProjectCategoriesEnabled));
            OnPropertyChanged(nameof(IsProjectsEnabled));
            OnPropertyChanged(nameof(IsConsumersEnabled));
            OnPropertyChanged(nameof(IsUsersManagementEnabled));
            OnPropertyChanged(nameof(IsBusinessBalanceEnabled));
            OnPropertyChanged(nameof(IsLocationsEnabled));
        }

        // Optional: Async initialization if needed
        public async Task InitializeAsync()
        {
            try
            {
                // If any ViewModels need async initialization
                await Task.WhenAll(
                    WelcomeVM.LoadInitialDataAsync(),
                    BusinessBalanceVM.LoadInitialDataAsync()
                // Add other async initializations as needed
                );
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Handle initialization errors
                    new ErrorDialog("Initialization Error", "Failed to initialize application data");
                });
            }
        }
    }
}