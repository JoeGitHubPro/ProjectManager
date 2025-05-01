using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.Windows;

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
            LoadData();
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

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand AddNewCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(SaveCategory, () => !HasErrors);
            DeleteCommand = new RelayCommand(DeleteCategory, () => CanDelete);
            RefreshCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(FilterCategories);
            AddNewCommand = new RelayCommand(AddNewCategory);
            EditCommand = new RelayCommand(EditCategory);
            ClearCommand = new RelayCommand(ClearFields);
        }

        private void EditCategory()
        {
            IsFormVisible = true;
        }

        #endregion

        #region Methods

        private void LoadData()
        {
            ClearFields();

            try
            {
                _context = new AppDbContext();

                var categories = _context.ProjectCategories.ToList();
                ProjectCategories = new ObservableCollection<ProjectCategory>(categories);
                FilteredCategories = new ObservableCollection<ProjectCategory>(categories);

                OnPropertyChanged(nameof(ProjectCategories));
                OnPropertyChanged(nameof(FilteredCategories));
            }
            catch (Exception ex)
            {
                ShowError($"Error loading data: {ex.Message}");
            }
        }

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

        private void SaveCategory()
        {
            if (HasErrors || string.IsNullOrWhiteSpace(CategoryName))
            {
                ShowWarning("Please fix validation errors before saving.");
                return;
            }

            try
            {
                if (_isNewRecord)
                {
                    SaveNewCategory();
                }
                else
                {
                    UpdateExistingCategory();
                }

                _context.SaveChanges();
                LoadData();
                FilterCategories();

                ShowInfo("Category saved successfully.");
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Error saving category: {ex.Message}");
            }
        }

        private void SaveNewCategory()
        {
            var existing = _context.ProjectCategories.FirstOrDefault(c => c.Name == CategoryName);
            if (existing != null)
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }

            var category = new ProjectCategory { Name = CategoryName };
            _context.ProjectCategories.Add(category);
            ProjectCategories.Add(category);
        }

        private void UpdateExistingCategory()
        {
            if (SelectedCategory == null)
                return;

            SelectedCategory.Name = CategoryName;
            _context.Entry(SelectedCategory).State = EntityState.Modified;
        }

        private void DeleteCategory()
        {
            if (SelectedCategory == null)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this category? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _context.ProjectCategories.Remove(SelectedCategory);
                _context.SaveChanges();

                ProjectCategories.Remove(SelectedCategory);
                FilteredCategories.Remove(SelectedCategory);
                ClearFields();

                ShowInfo("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting category: {ex.Message}");
            }
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

        public Dictionary<string, string> ErrorCollection
        {
            get => _errorCollection;
        }

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

        #region Helper Methods

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
