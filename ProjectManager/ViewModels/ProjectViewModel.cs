using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ProjectViewModel : BaseViewModel
    {
        private AppDbContext _context;

        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<ProjectCategory> ProjectCategories { get; set; }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ProjectViewModel()
        {
            _context = new AppDbContext();
            Projects = new ObservableCollection<Project>(_context.Projects.ToList());
            ProjectCategories = new ObservableCollection<ProjectCategory>(_context.ProjectCategories.ToList());

            SelectedProject = new Project();

            AddCommand = new RelayCommand(AddProject);
            UpdateCommand = new RelayCommand(UpdateProject, () => SelectedProject != null && SelectedProject.Id != 0);
            DeleteCommand = new RelayCommand(DeleteProject, () => SelectedProject != null && SelectedProject.Id != 0);
        }

        private void AddProject()
        {
            _context.Projects.Add(SelectedProject);
            _context.SaveChanges();
            Projects.Add(SelectedProject);
            SelectedProject = new Project();
        }

        private void UpdateProject()
        {
            _context.Projects.Update(SelectedProject);
            _context.SaveChanges();
        }

        private void DeleteProject()
        {
            _context.Projects.Remove(SelectedProject);
            _context.SaveChanges();
            Projects.Remove(SelectedProject);
            SelectedProject = new Project();
        }
    }
}
