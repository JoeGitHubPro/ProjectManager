using ProjectManager.ViewModels.Identities;
using System.Windows;
using System.Windows;
using System.Windows.Controls;

namespace ProjectManager.Views.Identities
{
    /// <summary>
    /// Interaction logic for UserManagementWindow.xaml
    /// </summary>
    public partial class UserManagementWindow : Window
    {
        public UserManagementWindow()
        {
            InitializeComponent();
            DataContext = new UserManagementViewModel(this);
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true; // Open the menu on left-click
            }
        }

    }
}
