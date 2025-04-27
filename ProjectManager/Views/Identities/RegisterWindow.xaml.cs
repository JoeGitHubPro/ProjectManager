using ProjectManager.ViewModels.Identities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectManager.Views.Identities
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {

        public RegisterWindow()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel(this);
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EmailTextBox.Focus();
            }
        }

        private void EmailTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PhoneNumberTextBox.Focus();
            }
        }

        private void PhoneNumberTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmPasswordPasswordBox.Focus();
            }
        }

        private void ConfirmPasswordPasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChooseRoleComboBox.Focus();
            }
        }


    }
}
