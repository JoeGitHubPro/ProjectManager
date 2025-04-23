namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ConsumerViewModel ConsumerVM { get; set; }
        public ProjectCategoryViewModel ProjectCategoryVM { get; set; }
        public ProjectViewModel ProjectVM { get; set; }
        public ProjectOrderViewModel ProjectOrderVM { get; set; }

        public MainViewModel()
        {
            ConsumerVM = new ConsumerViewModel();
            ProjectCategoryVM = new ProjectCategoryViewModel();
            ProjectVM = new ProjectViewModel();
            ProjectOrderVM = new ProjectOrderViewModel();
        }
    }

}
