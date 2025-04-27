using ProjectManager.ViewModels.Identities;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.Views.Identities
{
    /// <summary>
    /// Interaction logic for EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        public EditUserWindow(UserViewModel user)
        {
            InitializeComponent();
            DataContext = new EditUserViewModel(user, this);
        }

        private void UsernameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EmailTextBox.Focus();
            }
        }

        private void EmailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PhoneTextBox.Focus();
            }
        }

        private void PhoneTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoleComboBox.Focus();
            }
        }
    }
}
