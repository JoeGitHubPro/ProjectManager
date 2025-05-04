using ProjectManager.ViewModels.Database;
using System.Windows;

namespace ProjectManager.Views.Database
{
    /// <summary>
    /// Interaction logic for BackupWindow.xaml
    /// </summary>
    public partial class BackupWindow : Window
    {
        public BackupWindow()
        {
            InitializeComponent();
            DataContext = new BackupViewModel();
        }
    }
}
