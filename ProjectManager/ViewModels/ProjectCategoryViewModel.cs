using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ProjectCategoryViewModel : BaseViewModel
    {
        private readonly AppDbContext _context = new();

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));
                }
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
            }
        }

        public ObservableCollection<ProjectCategory> ProjectCategories { get; set; } = new();

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ProjectCategoryViewModel()
        {
            LoadData();
            AddCommand = new RelayCommand(Add);
            UpdateCommand = new RelayCommand(Update, () => SelectedCategory != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedCategory != null);
        }

        private void LoadData()
        {
            ProjectCategories.Clear();
            foreach (var category in _context.ProjectCategories)
                ProjectCategories.Add(category);
        }

        private void Add()
        {
            var newCategory = new ProjectCategory { Name = CategoryName };
            _context.ProjectCategories.Add(newCategory);
            _context.SaveChanges();
            CategoryName = null;
            LoadData();
        }

        private void Update()
        {
            if (SelectedCategory == null) return;
            _context.ProjectCategories.Update(SelectedCategory);
            _context.SaveChanges();
            LoadData();
        }

        private void Delete()
        {
            if (SelectedCategory == null) return;
            _context.ProjectCategories.Remove(SelectedCategory);
            _context.SaveChanges();
            LoadData();
        }

    }
}
