using ProjectManager.Models;
using ProjectManager.Screens;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class EditProjectViewModel : BaseViewModel
    {
        private ProjectInfo _selectedProject;

        public ProjectInfo SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private readonly AppDbContext _context;

        public EditProjectViewModel(ProjectInfo selectedProject)
        {
            _context = new AppDbContext();
            SelectedProject = selectedProject;

            SaveCommand = new RelayCommand(SaveProject);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveProject()
        {
            SelectedProject.ModifiedDate = DateTime.Now;
            _context.ProjectInfos.Update(SelectedProject);
            _context.SaveChanges();

            MessageBox.Show("Project edited successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Cancel();
        }

        private void Cancel()
        {
            // Optionally close the Add screen
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is EditProjectScreen)?
                .Close();
        }
    }
}
