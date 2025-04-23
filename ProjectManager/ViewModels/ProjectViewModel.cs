using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class ProjectViewModel : BaseViewModel, IDataErrorInfo
    {
        private AppDbContext _context;
        private bool _isBusy;
        private string _searchText = string.Empty;

        public ObservableCollection<Project> Projects { get; set; } = new();
        public ObservableCollection<Project> FilteredProjects { get; set; } = new();
        public ObservableCollection<ProjectCategory> ProjectCategories { get; set; } = new();

        public ProjectViewModel()
        {
            _context = new AppDbContext();

            // Initialize commands first
            SaveCommand = new RelayCommand(
                _ => SaveProject(),
                _ => !IsBusy && !HasErrors);

            DeleteCommand = new RelayCommand(
                _ => DeleteProject(),
                _ => !IsBusy && CanDelete);

            RefreshCommand = new RelayCommand(
                _ => LoadData(),
                _ => !IsBusy);

            SearchCommand = new RelayCommand(
                _ => FilterProjects(),
                _ => !IsBusy);

            AddNewCommand = new RelayCommand(
                _ => AddNewProject(),
                _ => !IsBusy);

            EditCommand = new RelayCommand(
                project => SelectedProject = project as Project,
                _ => !IsBusy);

            ClearCommand = new RelayCommand(
                _ => ClearFields(),
                _ => !IsBusy);

            LoadData();
        }

        #region Properties

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _details = string.Empty;
        public string Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged();
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private int _projectCategoryId;
        public int ProjectCategoryId
        {
            get => _projectCategoryId;
            set
            {
                _projectCategoryId = value;
                OnPropertyChanged();
                SaveCommand?.RaiseCanExecuteChanged();
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
                if (value != null)
                {
                    Name = value.Name;
                    Details = value.Details;
                    Price = value.Price;
                    ProjectCategoryId = value.ProjectCategoryId;
                }
                else
                {
                    Name = string.Empty;
                    Details = string.Empty;
                    Price = 0;
                    ProjectCategoryId = 0;
                }
                SaveCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
            }
        }

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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();

                // Null-conditional checks for commands
                SaveCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
                RefreshCommand?.RaiseCanExecuteChanged();
                SearchCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool CanDelete => SelectedProject != null;

        #endregion

        #region Commands

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand AddNewCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }

        #endregion

        #region Methods

        private void LoadData()
        {
            try
            {
                IsBusy = true;
                _context = new AppDbContext();

                var projects = _context.Projects.Include(p => p.ProjectCategory).ToList();
                Projects = new ObservableCollection<Project>(projects);
                FilteredProjects = new ObservableCollection<Project>(projects);

                ProjectCategories.Clear();
                foreach (var category in _context.ProjectCategories.ToList())
                {
                    ProjectCategories.Add(category);
                }

                OnPropertyChanged(nameof(Projects));
                OnPropertyChanged(nameof(FilteredProjects));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void RefreshData()
        {
            try
            {
                IsBusy = true;
                LoadData();
                MessageBox.Show("Data refreshed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterProjects()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProjects = new ObservableCollection<Project>(Projects);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredProjects = new ObservableCollection<Project>(
                    Projects.Where(p =>
                        p.Name.ToLower().Contains(searchLower) ||
                        (p.Details != null && p.Details.ToLower().Contains(searchLower)) ||
                        (p.ProjectCategory != null && p.ProjectCategory.Name.ToLower().Contains(searchLower)))
                );
            }
            OnPropertyChanged(nameof(FilteredProjects));
        }

        private void AddNewProject()
        {
            SelectedProject = null;
            Name = string.Empty;
            Details = string.Empty;
            Price = 0;
            ProjectCategoryId = 0;
        }

        private void SaveProject()
        {
            try
            {
                IsBusy = true;

                if (HasErrors)
                {
                    MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedProject == null)
                {
                    // Add new project
                    var existing = _context.Projects.FirstOrDefault(p => p.Name == Name);
                    if (existing != null)
                    {
                        MessageBox.Show("A project with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var project = new Project
                    {
                        Name = Name,
                        Details = Details,
                        Price = Price,
                        ProjectCategoryId = ProjectCategoryId
                    };

                    _context.Projects.Add(project);
                    Projects.Add(project);
                }
                else
                {
                    // Update existing project
                    SelectedProject.Name = Name;
                    SelectedProject.Details = Details;
                    SelectedProject.Price = Price;
                    SelectedProject.ProjectCategoryId = ProjectCategoryId;
                    _context.Entry(SelectedProject).State = EntityState.Modified;
                }

                _context.SaveChanges();
                FilterProjects();
                MessageBox.Show("Project saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DeleteProject()
        {
            try
            {
                if (SelectedProject == null) return;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this project? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                IsBusy = true;

                _context.Projects.Remove(SelectedProject);
                _context.SaveChanges();
                Projects.Remove(SelectedProject);
                FilterProjects();
                ClearFields();

                MessageBox.Show("Project deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearFields()
        {
            SelectedProject = null;
            Name = string.Empty;
            Details = string.Empty;
            Price = 0;
            ProjectCategoryId = 0;
        }

        #endregion

        #region Validation

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Project name is required";
                        if (Name.Length > 100)
                            return "Name cannot exceed 100 characters";
                        break;

                    case nameof(Details):
                        if (string.IsNullOrWhiteSpace(Details))
                            return "Project details are required";
                        if (Details.Length > 500)
                            return "Details cannot exceed 500 characters";
                        break;

                    case nameof(Price):
                        if (Price <= 0)
                            return "Price must be greater than 0";
                        break;

                    case nameof(ProjectCategoryId):
                        if (ProjectCategoryId == 0)
                            return "Project category is required";
                        break;
                }

                return string.Empty;
            }
        }

        public bool HasErrors
        {
            get
            {
                return !string.IsNullOrEmpty(this[nameof(Name)]) ||
                       !string.IsNullOrEmpty(this[nameof(Details)]) ||
                       !string.IsNullOrEmpty(this[nameof(Price)]) ||
                       !string.IsNullOrEmpty(this[nameof(ProjectCategoryId)]);
            }
        }

        #endregion
    }
}