using ProjectManager.ViewModels.Dialogs;
using System.Windows;

namespace ProjectManager.Views.Dialogs
{
    public partial class ErrorDialog : Window
    {
        public ErrorDialog(string errorMessage, string details)
        {
            InitializeComponent();
            DataContext = new ErrorDialogViewModel(this, errorMessage, details);
            ShowDialog();
        }
    }
}
