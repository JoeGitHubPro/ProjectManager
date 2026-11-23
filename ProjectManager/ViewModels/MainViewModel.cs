using Microsoft.Extensions.DependencyInjection;
using ProjectManager.Constants;
using ProjectManager.ViewModels.CapitalManagement;
using ProjectManager.ViewModels.Identities;
using ProjectManager.Views.Dialogs;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

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
        public bool IsConsumersEnabled { get; private set; } = true;
        public bool IsUsersManagementEnabled { get; private set; }
        public bool IsLocationsEnabled { get; private set; } = true;
        public bool IsBusinessBalanceEnabled { get; private set; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeViewModels();
            SetupRoleBasedAccess();
        }

        private void InitializeViewModels()
        {
            // Get all ViewModels from DI container
            ConsumerVM = _serviceProvider.GetRequiredService<ConsumerViewModel>();
            ProjectCategoryVM = _serviceProvider.GetRequiredService<ProjectCategoryViewModel>();
            ProjectVM = _serviceProvider.GetRequiredService<ProjectViewModel>();
            ProjectOrderVM = _serviceProvider.GetRequiredService<ProjectOrderViewModel>();
            UserManagementVM = _serviceProvider.GetRequiredService<UserManagementViewModel>();
            BusinessBalanceVM = _serviceProvider.GetRequiredService<BusinessBalanceViewModel>();
            LocationVM = _serviceProvider.GetRequiredService<LocationViewModel>();
            WelcomeVM = _serviceProvider.GetRequiredService<WelcomeViewModel>();
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
                    break;

                case Roles.Manager:
                    IsProjectOrderEnabled = true;
                    IsProjectCategoriesEnabled = true;
                    IsProjectsEnabled = true;
                    IsBusinessBalanceEnabled = true;
                    break;

                default:
                    IsProjectOrderEnabled = false;
                    IsProjectCategoriesEnabled = false;
                    IsProjectsEnabled = false;
                    IsBusinessBalanceEnabled = false;
                    break;
            }

            // Notify property changes
            OnPropertyChanged(nameof(IsProjectOrderEnabled));
            OnPropertyChanged(nameof(IsProjectCategoriesEnabled));
            OnPropertyChanged(nameof(IsProjectsEnabled));
            OnPropertyChanged(nameof(IsConsumersEnabled));
            OnPropertyChanged(nameof(IsUsersManagementEnabled));
            OnPropertyChanged(nameof(IsBusinessBalanceEnabled));
            OnPropertyChanged(nameof(IsLocationsEnabled));
        }

        public async Task InitializeAsync()
        {
            try
            {
                await Task.WhenAll(
                    WelcomeVM.LoadInitialDataAsync(),
                    BusinessBalanceVM.LoadInitialDataAsync(),
                    ConsumerVM.LoadDataAsync() // Initialize consumer data
                                               // Add other async initializations as needed
                );
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    new ErrorDialog("Initialization Error", $"Failed to initialize application data: {ex.Message}").ShowDialog();
                });
            }
        }
    }
}