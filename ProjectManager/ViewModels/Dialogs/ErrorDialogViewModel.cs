using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Dialogs
{
    public class ErrorDialogViewModel : BaseViewModel
    {
        public string ErrorMessage { get; }
        public string ErrorDetails { get; }
        public ICommand CloseCommand { get; }

        private Window _window;

        public ErrorDialogViewModel(Window window, string errorMessage, string details)
        {
            _window = window;
            ErrorMessage = errorMessage;
            ErrorDetails = details;
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void CloseWindow()
        {
            _window.Close();
        }
    }
}
