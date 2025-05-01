using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
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
                _searchText = value ?? string.Empty;
                OnPropertyChanged();
                FilterProjects();
            }
        }

        private Project _selectedProject = new();
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value ?? new Project();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
        }

        public bool CanDelete =>
            SelectedProject != null &&
            !string.IsNullOrWhiteSpace(SelectedProject.Name) &&
            _context.Projects.Any(a => a.Name == SelectedProject.Name);

        // Commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand AddNewCommand { get; }
        public RelayCommand EditCommand { get; }

        public ProjectViewModel()
        {
            SaveCommand = new RelayCommand(SaveProject, () => !HasErrors);
            DeleteCommand = new RelayCommand(DeleteProject, () => !HasErrors && CanDelete);
            RefreshCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(FilterProjects);
            AddNewCommand = new RelayCommand(AddNewProject);
            EditCommand = new RelayCommand(EditCategory);

            LoadData();
        }

        private void EditCategory()
        {
            IsFormVisible = true;
        }

        private void LoadData()
        {
            Projects.Clear();
            FilteredProjects.Clear();
            ProjectCategories.Clear();

            var projects = _context.Projects.Include(p => p.ProjectCategory).ToList();
            foreach (var p in projects)
            {
                Projects.Add(p);
                FilteredProjects.Add(p);
            }

            foreach (var c in _context.ProjectCategories.ToList())
                ProjectCategories.Add(c);

            SelectedProject = new Project();
            IsFormVisible = false;
        }

        private void FilterProjects()
        {
            FilteredProjects.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var p in Projects)
                    FilteredProjects.Add(p);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();
                var filtered = Projects.Where(p =>
                    (!string.IsNullOrWhiteSpace(p?.Name) && p.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrWhiteSpace(p?.Details) && p.Details.ToLower().Contains(lowerSearch)));

                foreach (var p in filtered)
                    FilteredProjects.Add(p);
            }
        }

        private void SaveProject()
        {
            if (SelectedProject == null)
                return;

            if (HasErrors)
            {
                new ErrorDialog("Validation Error",
                        $"{this[nameof(SelectedProject.Name)] ?? ""}\n" +
                        $"{this[nameof(SelectedProject.Details)] ?? ""}\n" +
                        $"{this[nameof(SelectedProject.Price)] ?? ""}\n" +
                        $"{this[nameof(SelectedProject.ProjectCategoryId)] ?? ""}");
                return;
            }

            if (SelectedProject.Id == 0)
                _context.Projects.Add(SelectedProject);
            else
                _context.Projects.Update(SelectedProject);

            _context.SaveChanges();
            LoadData();
        }

        private void DeleteProject()
        {
            if (SelectedProject == null || string.IsNullOrWhiteSpace(SelectedProject.Name))
                return;

            var result = new ConfirmationDialog(
                $"Are you sure you want to delete the project: {SelectedProject.Name}?");
            result.ShowDialog();

            if (result.IsConfirmed)
            {
                _context.Projects.Remove(SelectedProject);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void AddNewProject()
        {
            SelectedProject = new Project();
            IsFormVisible = true;
        }

        public string this[string columnName]
        {
            get
            {
                if (SelectedProject == null)
                    return string.Empty;

                return columnName switch
                {
                    nameof(SelectedProject.Name) =>
                        string.IsNullOrWhiteSpace(SelectedProject.Name) ? "Project name is required." : null!,

                    nameof(SelectedProject.Details) =>
                        string.IsNullOrWhiteSpace(SelectedProject.Details) ? "Project details are required." : null!,

                    nameof(SelectedProject.Price) =>
                        SelectedProject.Price <= 0 ? "Price must be greater than zero." : null!,

                    nameof(SelectedProject.ProjectCategoryId) =>
                        SelectedProject.ProjectCategoryId == 0 ? "Project category is required." : null!,

                    _ => null!
                };
            }
        }

        public bool HasErrors =>
            SelectedProject == null ||
            this[nameof(SelectedProject.Name)] != null ||
            this[nameof(SelectedProject.Price)] != null ||
            this[nameof(SelectedProject.ProjectCategoryId)] != null;
    }
}
