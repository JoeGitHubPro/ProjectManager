using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;

namespace ProjectManager.ViewModels
{
    public class ProjectViewModel : BaseViewModel
    {
        private readonly AppDbContext _context = new();

        public ObservableCollection<Project> Projects { get; set; } = new();
        public ObservableCollection<Project> FilteredProjects { get; set; } = new();
        public ObservableCollection<ProjectCategory> ProjectCategories { get; set; } = new();

        private string _searchText = string.Empty;
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

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        public bool CanDelete => SelectedProject != null;

        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand AddNewCommand { get; }

        public ProjectViewModel()
        {
            SaveCommand = new RelayCommand(SaveProject, () => !HasErrors);
            DeleteCommand = new RelayCommand(DeleteProject, () => CanDelete);
            RefreshCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(FilterProjects);
            AddNewCommand = new RelayCommand(AddNewProject);

            LoadData();
        }

        private void LoadData()
        {

            Projects.Clear();
            FilteredProjects.Clear();
            ProjectCategories.Clear();

            var projects = _context.Projects.Include(p => p.ProjectCategory).ToList();
            foreach (var p in projects)
                Projects.Add(p);

            foreach (var p in projects)
                FilteredProjects.Add(p);

            foreach (var c in _context.ProjectCategories.ToList())
                ProjectCategories.Add(c);

            SelectedProject = null;
        }

        private void FilterProjects()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProjects.Clear();
                foreach (var p in Projects)
                    FilteredProjects.Add(p);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();
                var filtered = Projects.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Name) && p.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrWhiteSpace(p.Details) && p.Details.ToLower().Contains(lowerSearch)));

                FilteredProjects.Clear();
                foreach (var p in filtered)
                    FilteredProjects.Add(p);
            }
        }

        private void SaveProject()
        {
            if (SelectedProject == null) return;

            if (SelectedProject.Id == 0)
                _context.Projects.Add(SelectedProject);
            else
                _context.Projects.Update(SelectedProject);

            _context.SaveChanges();
            LoadData();
        }

        private void DeleteProject()
        {
            if (SelectedProject == null) return;

            _context.Projects.Remove(SelectedProject);
            _context.SaveChanges();
            LoadData();
        }

        private void AddNewProject()
        {
            SelectedProject = new Project();
        }

        // --- Validation logic ---
        public Dictionary<string, string> ErrorCollection =>
   new Dictionary<string, string>
   {
       [nameof(SelectedProject.Name)] = this[nameof(SelectedProject.Name)],
       [nameof(SelectedProject.Details)] = this[nameof(SelectedProject.Details)],
       [nameof(SelectedProject.Price)] = this[nameof(SelectedProject.Price)],
       [nameof(SelectedProject.ProjectCategoryId)] = this[nameof(SelectedProject.ProjectCategoryId)]
   };
        public string this[string columnName]
        {
            get
            {
                if (SelectedProject == null) return string.Empty;

                return columnName switch
                {
                    nameof(SelectedProject.Name) =>
                        string.IsNullOrWhiteSpace(SelectedProject.Name) ? "Project name is required." : null,

                    nameof(SelectedProject.Details) =>
                        string.IsNullOrWhiteSpace(SelectedProject.Details) ? "Project details are required." : null,

                    nameof(SelectedProject.Price) =>
                        SelectedProject.Price <= 0 ? "Price must be greater than zero." : null,

                    nameof(SelectedProject.ProjectCategoryId) =>
                        SelectedProject.ProjectCategoryId == 0 ? "Project category is required." : null,

                    _ => null
                };
            }
        }

        public bool HasErrors =>
            this[nameof(SelectedProject.Name)] != null ||
            this[nameof(SelectedProject.Details)] != null ||
            this[nameof(SelectedProject.Price)] != null ||
            this[nameof(SelectedProject.ProjectCategoryId)] != null;
    }
}
