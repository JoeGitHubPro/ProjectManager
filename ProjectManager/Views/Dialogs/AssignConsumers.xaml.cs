using ProjectManager.ViewModels.Dialogs;
using System.Windows;

namespace ProjectManager.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AssignConsumers.xaml
    /// </summary>
    public partial class AssignConsumers : Window
    {
        public string? SelectedUserId { get; private set; }

        public AssignConsumers()
        {
            InitializeComponent();
            var viewModel = new AssignConsumersViewModel();
            viewModel.Assigned += userId =>
            {
                SelectedUserId = userId;
                DialogResult = true;
                Close();
            };
            DataContext = viewModel;
        }
    }
}
