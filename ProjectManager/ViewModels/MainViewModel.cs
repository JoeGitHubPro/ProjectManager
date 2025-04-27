using ProjectManager.Constants;

namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ConsumerViewModel ConsumerVM { get; set; }
        public ProjectCategoryViewModel ProjectCategoryVM { get; set; }
        public ProjectViewModel ProjectVM { get; set; }
        public ProjectOrderViewModel ProjectOrderVM { get; set; }

        // Add properties to control tab enable state
        public bool IsProjectOrderEnabled { get; set; } = true;
        public bool IsProjectCategoriesEnabled { get; set; } = true;
        public bool IsProjectsEnabled { get; set; } = true;
        public bool IsConsumersEnabled { get; set; } = true;

        public MainViewModel()
        {
            ConsumerVM = new ConsumerViewModel();
            ProjectCategoryVM = new ProjectCategoryViewModel();
            ProjectVM = new ProjectViewModel();
            ProjectOrderVM = new ProjectOrderViewModel();

            // Set your conditions here
            // Example: Disable Consumers tab
            if (UserSession.Role != Roles.Admin)
            {
                IsConsumersEnabled = false;
            }
        }
    }
}