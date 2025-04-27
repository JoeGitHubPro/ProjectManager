using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class ProjectOrderViewModel : BaseViewModel, IDataErrorInfo
    {
        private AppDbContext _context;

        public ObservableCollection<ProjectOrder> ProjectOrders { get; set; } = new();
        public ObservableCollection<Consumer> Consumers { get; set; } = new();
        public ObservableCollection<Project> Projects { get; set; } = new();

        #region Properties
        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged();
                    UpdateTotalPrice();
                }
            }
        }

        private string _installationSystemInfo = string.Empty;
        public string InstallationSystemInfo
        {
            get => _installationSystemInfo;
            set
            {
                if (_installationSystemInfo != value)
                {
                    _installationSystemInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _soldDate = DateTime.Today;
        public DateTime SoldDate
        {
            get => _soldDate;
            set
            {
                if (_soldDate != value)
                {
                    _soldDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedConsumerId;
        public int SelectedConsumerId
        {
            get => _selectedConsumerId;
            set
            {
                if (_selectedConsumerId != value)
                {
                    _selectedConsumerId = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _selectedProjectId;
        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set
            {
                if (_selectedProjectId != value)
                {
                    _selectedProjectId = value;
                    OnPropertyChanged();
                    UpdateTotalPrice();
                }
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterData();
                }
            }
        }

        private ProjectOrder _selectedOrder;
        public ProjectOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanDelete));
                    if (_selectedOrder != null)
                    {
                        Discount = _selectedOrder.Discount;
                        InstallationSystemInfo = _selectedOrder.InstallationSystemInfo;
                        SoldDate = _selectedOrder.SoldDate;
                        Notes = _selectedOrder.Notes;
                        SelectedConsumerId = _selectedOrder.ConsumerId;
                        SelectedProjectId = _selectedOrder.ProjectId;
                    }
                }
            }
        }

        public bool CanDelete => SelectedOrder != null;
        #endregion

        #region Commands
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand ReloadPageCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand LocateCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand UpdateCommand { get; private set; }
        #endregion

        public ProjectOrderViewModel()
        {
            _context = new AppDbContext();
            InitializeCommands();
            LoadData();
        }

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(Save, () => !HasErrors);
            DeleteCommand = new RelayCommand(Delete, () => CanDelete);
            RefreshCommand = new RelayCommand(LoadData);
            ReloadPageCommand = new RelayCommand(() => { LoadData(); Clear(); });
            ClearCommand = new RelayCommand(Clear);
            LocateCommand = new RelayCommand(Locate);
            AddCommand = new RelayCommand(AddNewOrder);
            UpdateCommand = new RelayCommand(UpdateOrder, () => SelectedOrder != null && !HasErrors);
        }

        private void AddNewOrder()
        {
            SelectedOrder = null;
            Clear();
        }

        private void UpdateOrder()
        {
            Save();
        }

        private void LoadData()
        {
            try
            {
                _context?.Dispose();
                _context = new AppDbContext();

                ProjectOrders = new ObservableCollection<ProjectOrder>(
                    _context.ProjectOrders
                        .Include(p => p.Project)
                        .Include(p => p.Consumer)
                        .ToList());

                Consumers = new ObservableCollection<Consumer>(_context.Consumers.ToList());
                Projects = new ObservableCollection<Project>(_context.Projects.ToList());

                OnPropertyChanged(nameof(ProjectOrders));
                OnPropertyChanged(nameof(Consumers));
                OnPropertyChanged(nameof(Projects));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void FilterData()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                OnPropertyChanged(nameof(ProjectOrders));
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = new ObservableCollection<ProjectOrder>(
                _context.ProjectOrders
                    .Include(p => p.Project)
                    .Include(p => p.Consumer)
                    .Where(o =>
                        o.Project.Name.ToLower().Contains(searchLower) ||
                        o.Consumer.Phone.Contains(searchLower) ||
                        (o.Notes != null && o.Notes.ToLower().Contains(searchLower)) ||
                        o.FolderSerial.Contains(searchLower))
                    .ToList());

            ProjectOrders = filtered;
            OnPropertyChanged(nameof(ProjectOrders));
        }

        private void Save()
        {
            try
            {

                if (HasErrors)
                {
                    MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
                if (project == null)
                {
                    MessageBox.Show("Please select a valid project.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (SelectedOrder == null)
                {
                    // Add new order
                    var order = new ProjectOrder
                    {
                        FolderSerial = FolderHelper.CreateUniqueFolder(),
                        Discount = Discount,
                        InstallationSystemInfo = InstallationSystemInfo,
                        SoldDate = SoldDate,
                        Notes = Notes,
                        ConsumerId = SelectedConsumerId,
                        ProjectId = SelectedProjectId,
                        TotalPrice = project.Price * (1 - Discount / 100)
                    };

                    _context.ProjectOrders.Add(order);
                }
                else
                {
                    // Update existing order
                    SelectedOrder.Discount = Discount;
                    SelectedOrder.InstallationSystemInfo = InstallationSystemInfo;
                    SelectedOrder.SoldDate = SoldDate;
                    SelectedOrder.Notes = Notes;
                    SelectedOrder.ConsumerId = SelectedConsumerId;
                    SelectedOrder.ProjectId = SelectedProjectId;
                    SelectedOrder.TotalPrice = project.Price * (1 - Discount / 100);
                }

                _context.SaveChanges();
                LoadData();
                MessageBox.Show("Order saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Delete()
        {
            try
            {
                if (SelectedOrder == null) return;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this order? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                FolderHelper.DeleteFolder(SelectedOrder.FolderSerial);
                _context.ProjectOrders.Remove(SelectedOrder);
                _context.SaveChanges();
                LoadData();
                Clear();
                MessageBox.Show("Order deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear()
        {
            Discount = 0;
            InstallationSystemInfo = string.Empty;
            SoldDate = DateTime.Today;
            Notes = string.Empty;
            SelectedConsumerId = 0;
            SelectedProjectId = 0;
            SelectedOrder = null;
        }

        private void Locate()
        {
            try
            {
                if (SelectedOrder != null && !string.IsNullOrEmpty(SelectedOrder.FolderSerial))
                {
                    FolderHelper.OpenFolderByName(SelectedOrder.FolderSerial);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalPrice()
        {
            var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
            TotalPrice = project != null ? project.Price * (1 - Discount / 100) : 0;
        }

        #region Validation
        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(SelectedProjectId):
                        if (SelectedProjectId <= 0)
                            return "Please select a project";
                        break;

                    case nameof(Discount):
                        if (Discount < 0 || Discount > 100)
                            return "Discount must be between 0 and 100";
                        break;


                    case nameof(SoldDate):
                        if (SoldDate > DateTime.Today)
                            return "Sold date cannot be in the future";
                        break;

                    case nameof(SelectedConsumerId):
                        if (SelectedConsumerId <= 0)
                            return "Consumer is required";
                        break;
                }
                return string.Empty;
            }
        }

        private bool HasErrors
        {
            get
            {
                return !string.IsNullOrEmpty(this[nameof(SelectedProjectId)]) ||
                       !string.IsNullOrEmpty(this[nameof(Discount)]) ||
                       !string.IsNullOrEmpty(this[nameof(SoldDate)]) ||
                       !string.IsNullOrEmpty(this[nameof(SelectedConsumerId)]);
            }
        }
        #endregion
    }
}