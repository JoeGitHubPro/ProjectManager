using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

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
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }

        public ProjectViewModel()
        {
            SaveCommand = new AsyncRelayCommand(SaveProjectAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteProjectAsync, () => !HasErrors && CanDelete);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchCommand = new RelayCommand(FilterProjects);
            AddNewCommand = new RelayCommand(AddNewProject);
            EditCommand = new RelayCommand(EditCategory);

            _ = LoadDataAsync(); // Fire-and-forget async initialization
        }

        private void EditCategory()
        {
            IsFormVisible = true;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Projects.Clear();
                    FilteredProjects.Clear();
                    ProjectCategories.Clear();
                });

                var projects = await _context.Projects
                    .Include(p => p.ProjectCategory)
                    .ToListAsync();

                var categories = await _context.ProjectCategories.ToListAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var p in projects)
                    {
                        Projects.Add(p);
                        FilteredProjects.Add(p);
                    }

                    foreach (var c in categories)
                        ProjectCategories.Add(c);

                    SelectedProject = new Project();
                    IsFormVisible = false;
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading data", $" {ex.Message}");
            }
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

        private async Task SaveProjectAsync()
        {
            if (SelectedProject == null)
                return;

            if (HasErrors)
            {
                await ShowErrorAsync("Validation Error",
                    $"{this[nameof(SelectedProject.Name)] ?? ""}\n" +
                    $"{this[nameof(SelectedProject.Details)] ?? ""}\n" +
                    $"{this[nameof(SelectedProject.Price)] ?? ""}\n" +
                    $"{this[nameof(SelectedProject.ProjectCategoryId)] ?? ""}");
                return;
            }

            try
            {
                if (SelectedProject.Id == 0)
                    await _context.Projects.AddAsync(SelectedProject);
                else
                    _context.Projects.Update(SelectedProject);

                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving project", ex.Message);
            }
        }

        private async Task DeleteProjectAsync()
        {
            if (SelectedProject == null || string.IsNullOrWhiteSpace(SelectedProject.Name))
                return;

            var confirmed = await ShowConfirmationDialogAsync(
                $"Are you sure you want to delete the project: {SelectedProject.Name}?");

            if (confirmed)
            {
                try
                {
                    _context.Projects.Remove(SelectedProject);
                    await _context.SaveChangesAsync();
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync("Error deleting project", ex.Message);
                }
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
        private async Task<bool> ShowConfirmationDialogAsync(string message)
        {
            bool? result = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog(message);
                return dialog.ShowDialog();
            });

            return result == true;
        }
        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }
    }
}