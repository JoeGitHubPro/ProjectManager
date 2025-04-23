using ProjectManager.Models;
using ProjectManager.Screens;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _searchText;
        private ObservableCollection<ProjectInfo> _projects;
        private ObservableCollection<ProjectInfo> _filteredProjects;
        private ProjectInfo _selectedProject;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProjects();
            }
        }

        public ObservableCollection<ProjectInfo> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
                FilterProjects();
            }
        }

        public ObservableCollection<ProjectInfo> FilteredProjects
        {
            get => _filteredProjects;
            set
            {
                _filteredProjects = value;
                OnPropertyChanged();
            }
        }

        public ProjectInfo SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddNewProjectCommand { get; set; }
        public ICommand RefreshProjectsCommand { get; set; }
        public ICommand LocateCommand { get; set; }
        public ICommand EditProjectCommand { get; set; }
        public ICommand DeleteProjectCommand { get; set; }

        private readonly AppDbContext _context;

        public MainViewModel()
        {
            _context = new AppDbContext();
            LoadProjects();

            AddNewProjectCommand = new RelayCommand(AddNewProject);
            RefreshProjectsCommand = new RelayCommand(LoadProjects);
            LocateCommand = new RelayCommand(OpenFolderByName);
            EditProjectCommand = new RelayCommand(EditProject);
            DeleteProjectCommand = new RelayCommand(DeleteProject);
        }

        private void LoadProjects()
        {
            var projectInfos = _context.ProjectInfos.ToList();
            if (projectInfos is null)
                return;

            Projects = new ObservableCollection<ProjectInfo>();
        }

        private void FilterProjects()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProjects = new ObservableCollection<ProjectInfo>(Projects);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();

                FilteredProjects = new ObservableCollection<ProjectInfo>(
                    Projects.Where(p =>
                        p.ProjectId.ToString().Contains(lowerSearch) ||
                        (p.ProjectDetails?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.ProjectPrice?.ToString().Contains(lowerSearch) ?? false) ||
                        (p.FolderSerial?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.OwnerName?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.OwnerCommercialRegister?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.OwnerPhone?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.OwnerSystemDetails?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.Notes?.ToLower().Contains(lowerSearch) ?? false) ||
                        (p.CreatedDate.ToString("g").ToLower().Contains(lowerSearch)) ||
                        (p.ModifiedDate?.ToString("g").ToLower().Contains(lowerSearch) ?? false)
                    )
                );
            }
        }


        private void AddNewProject()
        {
            new AddProjectScreen().ShowDialog();
            LoadProjects();
        }

        private void EditProject()
        {
            if (SelectedProject != null)
            {
                new EditProjectScreen(SelectedProject).ShowDialog();
                LoadProjects();
            }
        }

        private void DeleteProject()
        {
            if (SelectedProject != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the project owned by \"{SelectedProject.OwnerName}\"?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.ProjectInfos.Remove(SelectedProject);
                    _context.SaveChanges();
                    Projects.Remove(SelectedProject);
                    SelectedProject = null;

                    MessageBox.Show("Project deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProjects();
                }
            }
        }

        private void OpenFolderByName()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dir");
            string targetPath = Path.Combine(baseDir, SelectedProject.FolderSerial);

            if (Directory.Exists(targetPath))
            {
                Process.Start("explorer.exe", targetPath);
            }
            else
            {
                System.Windows.MessageBox.Show("Folder not found!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
