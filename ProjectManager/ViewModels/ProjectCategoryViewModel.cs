using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ProjectCategoryViewModel : BaseViewModel
    {
        private AppDbContext _context;
        private string _searchText = string.Empty;
        private ProjectCategory _selectedCategory;
        private string _categoryName = string.Empty;
        private bool _isNewRecord = true;
        private Dictionary<string, string> _errorCollection = new();

        public ProjectCategoryViewModel()
        {
            InitializeCommands();
            _ = LoadDataAsync(); // Fire-and-forget async initialization
        }

        #region Properties
        public ObservableCollection<ProjectCategory> ProjectCategories { get; private set; } = new();
        public ObservableCollection<ProjectCategory> FilteredCategories { get; private set; } = new();

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value ?? string.Empty;
                OnPropertyChanged();
                ValidateProperty(nameof(CategoryName));
            }
        }

        public ProjectCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));

                if (value != null)
                {
                    _isNewRecord = false;
                    CategoryName = value.Name ?? string.Empty;
                }
                else
                {
                    CategoryName = string.Empty;
                    _isNewRecord = true;
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                OnPropertyChanged();
                FilterCategories();
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

        public bool CanDelete => SelectedCategory != null;
        #endregion

        #region Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand AddNewCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveCategoryAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteCategoryAsync, () => CanDelete);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchCommand = new RelayCommand(FilterCategories); // Remains synchronous
            AddNewCommand = new RelayCommand(AddNewCategory); // Remains synchronous
            EditCommand = new RelayCommand(EditCategory); // Remains synchronous
            ClearCommand = new RelayCommand(ClearFields); // Remains synchronous
        }

        private void EditCategory()
        {
            IsFormVisible = true;
        }
        #endregion

        #region Async Methods
        private async Task LoadDataAsync()
        {
            try
            {
                ClearFields();
                _context = new AppDbContext();

                var categories = await _context.ProjectCategories.ToListAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProjectCategories = new ObservableCollection<ProjectCategory>(categories);
                    FilteredCategories = new ObservableCollection<ProjectCategory>(categories);
                    OnPropertyChanged(nameof(ProjectCategories));
                    OnPropertyChanged(nameof(FilteredCategories));
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error loading data: {ex.Message}");
            }
        }

        private async Task SaveCategoryAsync()
        {
            if (HasErrors || string.IsNullOrWhiteSpace(CategoryName))
            {
                await ShowWarningAsync("Please fix validation errors before saving.");
                return;
            }

            try
            {
                if (_isNewRecord)
                {
                    await SaveNewCategoryAsync();
                }
                else
                {
                    await UpdateExistingCategoryAsync();
                }

                await _context.SaveChangesAsync();
                await LoadDataAsync();
                FilterCategories();

                await ShowInfoAsync("Category saved successfully.");
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error saving category: {ex.Message}");
            }
        }

        private async Task SaveNewCategoryAsync()
        {
            var existing = await _context.ProjectCategories
                .FirstOrDefaultAsync(c => c.Name == CategoryName);

            if (existing != null)
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }

            var category = new ProjectCategory { Name = CategoryName };
            await _context.ProjectCategories.AddAsync(category);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProjectCategories.Add(category);
            });
        }

        private async Task UpdateExistingCategoryAsync()
        {
            if (SelectedCategory == null) return;

            SelectedCategory.Name = CategoryName;
            _context.Entry(SelectedCategory).State = EntityState.Modified;
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            try
            {
                var confirmed = await ShowConfirmationDialogAsync(
                    "Are you sure you want to delete this category? This action cannot be undone.");

                if (!confirmed) return;

                _context.ProjectCategories.Remove(SelectedCategory);
                await _context.SaveChangesAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProjectCategories.Remove(SelectedCategory);
                    FilteredCategories.Remove(SelectedCategory);
                    ClearFields();
                });

                await ShowInfoAsync("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error deleting category: {ex.Message}");
            }
        }
        #endregion

        #region Synchronous Methods
        private void FilterCategories()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCategories = new ObservableCollection<ProjectCategory>(ProjectCategories);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredCategories = new ObservableCollection<ProjectCategory>(
                    ProjectCategories.Where(c => (c.Name ?? string.Empty).ToLower().Contains(searchLower))
                );
            }
            OnPropertyChanged(nameof(FilteredCategories));
        }

        private void AddNewCategory()
        {
            ClearFields();
            _isNewRecord = true;
            IsFormVisible = true;
        }

        private void ClearFields()
        {
            SelectedCategory = null;
            CategoryName = string.Empty;
            _isNewRecord = true;
            IsFormVisible = false;

            _errorCollection.Clear();
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }
        #endregion

        #region Validation
        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;

                switch (propertyName)
                {
                    case nameof(CategoryName):
                        if (string.IsNullOrWhiteSpace(CategoryName))
                            error = "Category name is required";
                        else if (CategoryName.Length > 100)
                            error = "Category name cannot exceed 100 characters";
                        break;
                }

                return error;
            }
        }

        public Dictionary<string, string> ErrorCollection => _errorCollection;

        private void ValidateProperty(string propertyName)
        {
            string error = this[propertyName] ?? string.Empty;

            if (_errorCollection.ContainsKey(propertyName))
                _errorCollection[propertyName] = error;
            else
                _errorCollection.Add(propertyName, error);

            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        public bool HasErrors => _errorCollection.Values.Any(error => !string.IsNullOrEmpty(error));
        #endregion

        #region Async Helper Methods
        private async Task ShowErrorAsync(string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog("Error", message);
            });
        }
        private async Task<bool> ShowConfirmationDialogAsync(string message)
        {
            bool? result = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog(message);
                return dialog.ShowDialog();
            });

            return result == true;
        }
        private async Task ShowWarningAsync(string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new WarningDialog(message);
            });
        }

        private async Task ShowInfoAsync(string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new InformationDialog(message);
            });
        }
        #endregion
    }
}