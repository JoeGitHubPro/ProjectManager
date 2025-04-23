using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class ProjectCategoryViewModel : BaseViewModel, IDataErrorInfo
    {
        private AppDbContext _context;
        private bool _isBusy;
        private string _searchText = string.Empty;
        public Dictionary<string, string> ErrorCollection =>
    new Dictionary<string, string>
    {
        [nameof(CategoryName)] = this[nameof(CategoryName)]
    };
        public ObservableCollection<ProjectCategory> ProjectCategories { get; set; } = new();
        public ObservableCollection<ProjectCategory> FilteredCategories { get; set; } = new();

        public ProjectCategoryViewModel()
        {
            _context = new AppDbContext();

            // Initialize commands first
            SaveCommand = new RelayCommand(
                _ => SaveCategory(),
                _ => !IsBusy && !HasErrors);

            DeleteCommand = new RelayCommand(
                _ => DeleteCategory(),
                _ => !IsBusy && CanDelete);

            RefreshCommand = new RelayCommand(
                _ => LoadData(),
                _ => !IsBusy);

            SearchCommand = new RelayCommand(
                _ => FilterCategories(),
                _ => !IsBusy);

            AddNewCommand = new RelayCommand(
                _ => AddNewCategory(),
                _ => !IsBusy);

            EditCommand = new RelayCommand(
                category => SelectedCategory = category as ProjectCategory,
                _ => !IsBusy);

            ClearCommand = new RelayCommand(
                _ => ClearFields(),
                _ => !IsBusy);

            LoadData();
        }

        #region Properties

        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private ProjectCategory _selectedCategory;
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
                    CategoryName = value.Name;
                }
                else
                {
                    CategoryName = string.Empty;
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
                FilterCategories();
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

        #endregion

        #region Methods

        private void LoadData()
        {
            try
            {
                IsBusy = true;
                _context = new AppDbContext();

                var categories = _context.ProjectCategories.ToList();
                ProjectCategories = new ObservableCollection<ProjectCategory>(categories);
                FilteredCategories = new ObservableCollection<ProjectCategory>(categories);

                OnPropertyChanged(nameof(ProjectCategories));
                OnPropertyChanged(nameof(FilteredCategories));
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
                    ProjectCategories.Where(c => c.Name.ToLower().Contains(searchLower))
                );
            }
            OnPropertyChanged(nameof(FilteredCategories));
        }

        private void AddNewCategory()
        {
            SelectedCategory = null;
            CategoryName = string.Empty;
        }

        private void SaveCategory()
        {
            try
            {
                IsBusy = true;

                if (HasErrors || string.IsNullOrWhiteSpace(CategoryName))
                {
                    MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCategory == null)
                {
                    // Add new category
                    var existing = _context.ProjectCategories.FirstOrDefault(c => c.Name == CategoryName);
                    if (existing != null)
                    {
                        MessageBox.Show("A category with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var category = new ProjectCategory { Name = CategoryName };
                    _context.ProjectCategories.Add(category);
                    ProjectCategories.Add(category);
                }
                else
                {
                    // Update existing category
                    SelectedCategory.Name = CategoryName;
                    _context.Entry(SelectedCategory).State = EntityState.Modified;
                }

                _context.SaveChanges();
                FilterCategories();
                MessageBox.Show("Category saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DeleteCategory()
        {
            try
            {
                if (SelectedCategory == null) return;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this category? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                IsBusy = true;

                _context.ProjectCategories.Remove(SelectedCategory);
                _context.SaveChanges();
                ProjectCategories.Remove(SelectedCategory);
                FilterCategories();
                ClearFields();

                MessageBox.Show("Category deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearFields()
        {
            SelectedCategory = null;
            CategoryName = string.Empty;
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
                    case nameof(CategoryName):
                        if (string.IsNullOrWhiteSpace(CategoryName))
                            return "Category name is required";
                        if (CategoryName.Length > 100)
                            return "Category name cannot exceed 100 characters";
                        break;
                }

                return string.Empty;
            }
        }

        public bool HasErrors
        {
            get
            {
                return !string.IsNullOrEmpty(this[nameof(CategoryName)]);
            }
        }

        #endregion
    }
}