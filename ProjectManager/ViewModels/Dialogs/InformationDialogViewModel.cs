using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Dialogs
{

    public class InformationDialogViewModel : BaseViewModel
    {
        public string Message { get; }
        public ICommand OkCommand { get; }

        private Window _window;

        public InformationDialogViewModel(Window window, string message)
        {
            _window = window;
            Message = message;
            OkCommand = new RelayCommand(CloseWindow);
        }

        private void CloseWindow()
        {
            _window.Close();
        }
    }
}
