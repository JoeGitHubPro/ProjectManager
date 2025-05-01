using System.Windows.Input;

namespace ProjectManager.ViewModels.Dialogs
{
    public class ConfirmationDialogViewModel : BaseViewModel
    {
        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsConfirmed { get; private set; }
        public Action<bool>? CloseDialog { get; set; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ConfirmationDialogViewModel(string message)
        {
            Message = message;
            ConfirmCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Confirm()
        {
            IsConfirmed = true;
            CloseDialog?.Invoke(true);
        }

        private void Cancel()
        {
            IsConfirmed = false;
            CloseDialog?.Invoke(false);
        }
    }
}
