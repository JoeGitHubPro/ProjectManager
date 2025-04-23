using ProjectManager.Models;
using ProjectManager.ViewModels;
using System.Windows;

namespace ProjectManager.Screens
{
    /// <summary>
    /// Interaction logic for EditProjectScreen.xaml
    /// </summary>
    public partial class EditProjectScreen : Window
    {
        public EditProjectScreen(ProjectInfo projectInfo)
        {
            InitializeComponent();
            DataContext = new EditProjectViewModel(projectInfo);
        }
    }
}
