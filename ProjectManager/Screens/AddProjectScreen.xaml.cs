using ProjectManager.ViewModels;
using System.Windows;

namespace ProjectManager.Screens
{
    /// <summary>
    /// Interaction logic for AddProjectScreen.xaml
    /// </summary>
    public partial class AddProjectScreen : Window
    {
        public AddProjectScreen()
        {
            InitializeComponent();
            DataContext = new AddProjectViewModel();
        }
    }
}
