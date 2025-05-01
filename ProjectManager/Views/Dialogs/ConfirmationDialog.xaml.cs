using ProjectManager.ViewModels.Dialogs;
using System.Windows;

namespace ProjectManager.Views.Dialogs
{
    public partial class ConfirmationDialog : Window
    {
        public bool IsConfirmed { get; private set; }

        public ConfirmationDialog(string message)
        {
            InitializeComponent();

            var viewModel = new ConfirmationDialogViewModel(message);

            viewModel.CloseDialog = result =>
            {
                IsConfirmed = viewModel.IsConfirmed;
                DialogResult = result;
            };

            DataContext = viewModel;
        }
    }
}
