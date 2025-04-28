using ProjectManager.Constants;
using ProjectManager.ViewModels.Identities;

namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ConsumerViewModel ConsumerVM { get; set; }
        public ProjectCategoryViewModel ProjectCategoryVM { get; set; }
        public ProjectViewModel ProjectVM { get; set; }
        public ProjectOrderViewModel ProjectOrderVM { get; set; }
        public UserManagementViewModel UserManagementVM { get; set; }

        // Add properties to control tab enable state
        public bool IsProjectOrderEnabled { get; set; }
        public bool IsProjectCategoriesEnabled { get; set; }
        public bool IsProjectsEnabled { get; set; }
        public bool IsConsumersEnabled { get; set; }
        public bool IsUsersManagementEnabled { get; set; }

        public MainViewModel()
        {
            ConsumerVM = new ConsumerViewModel();
            ProjectCategoryVM = new ProjectCategoryViewModel();
            ProjectVM = new ProjectViewModel();
            ProjectOrderVM = new ProjectOrderViewModel();
            UserManagementVM = new UserManagementViewModel();


            switch (UserSession.Role)
            {
                case Roles.Admin:
                    IsUsersManagementEnabled = true;
                    IsProjectOrderEnabled = true;
                    IsProjectCategoriesEnabled = true;
                    IsProjectsEnabled = true;
                    break;

                case Roles.Manager:
                    IsProjectOrderEnabled = true;
                    IsProjectCategoriesEnabled = true;
                    IsProjectsEnabled = true;
                    break;
            }

            // Enable consumers for all roles
            IsConsumersEnabled = true;


        }
    }
}