using Microsoft.AspNetCore.Identity;
using ProjectManager.Constants;
using ProjectManager.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels.Dialogs
{
    public class AssignConsumersViewModel : BaseViewModel
    {
        public ObservableCollection<IdentityUser> Users { get; set; }

        private IdentityUser _selectedUser;
        public IdentityUser SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public ICommand AssignCommand { get; }

        public event Action<string> Assigned;

        public AssignConsumersViewModel()
        {
            List<IdentityUser>? identityUsers;
            using (AuthenticationService service = new AuthenticationService())
            {
                identityUsers = Task.Run(() => service.GetUsersInRoleAsync(Roles.User)).Result;
            }
            Users = new ObservableCollection<IdentityUser>(identityUsers);
            AssignCommand = new RelayCommand(ExecuteAssign, CanAssign);
        }

        private void ExecuteAssign()
        {
            Assigned?.Invoke(SelectedUser.Id);
        }

        private bool CanAssign()
        {
            return SelectedUser != null;
        }
    }
}
