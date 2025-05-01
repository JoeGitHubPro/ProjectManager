using ProjectManager.ViewModels.Dialogs;
using System.Windows;

namespace ProjectManager.Views.Dialogs
{
    public partial class InformationDialog : Window
    {
        public InformationDialog(string message)
        {
            InitializeComponent();
            DataContext = new InformationDialogViewModel(this, message);
            ShowDialog();
        }
    }
}
