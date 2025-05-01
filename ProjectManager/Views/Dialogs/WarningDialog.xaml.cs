using ProjectManager.ViewModels.Dialogs;
using System.Windows;

namespace ProjectManager.Views.Dialogs
{
    public partial class WarningDialog : Window
    {
        public bool IsConfirmed { get; private set; }

        public WarningDialog(string message)
        {
            InitializeComponent();

            var viewModel = new WarningDialogViewModel(message);

            viewModel.CloseDialog = result =>
            {
                IsConfirmed = viewModel.IsConfirmed;
                DialogResult = result;
            };

            DataContext = viewModel;

            ShowDialog();
        }
    }
}
