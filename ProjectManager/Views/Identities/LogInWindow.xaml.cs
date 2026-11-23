using ProjectManager.ViewModels.Identities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectManager.Views.Identities
{
    /// <summary>
    /// Interaction logic for LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        public LogInWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = new LogInViewModel(this, serviceProvider);
            UsernameOrEmailTextBox.Focus();
        }

        private void UsernameOrEmailTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
                PasswordBox.SelectAll();
            }
        }

    }
}
