using Microsoft.EntityFrameworkCore;
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
        private bool _isBusy;

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
                    SaveCommand.RaiseCanExecuteChanged();
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
                    SaveCommand.RaiseCanExecuteChanged();
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
                    SaveCommand.RaiseCanExecuteChanged();
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

        private string _selectedConsumerPhone = string.Empty;
        public string SelectedConsumerPhone
        {
            get => _selectedConsumerPhone;
            set
            {
                if (_selectedConsumerPhone != value)
                {
                    _selectedConsumerPhone = value;
                    OnPropertyChanged();
                    SaveCommand.RaiseCanExecuteChanged();
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
                    SaveCommand.RaiseCanExecuteChanged();
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
                        SelectedConsumerPhone = _selectedOrder.ConsumerPhone;
                        SelectedProjectId = _selectedOrder.ProjectId;
                    }
                    SaveCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                    RefreshCommand.RaiseCanExecuteChanged();
                    ReloadPageCommand.RaiseCanExecuteChanged();
                    AddCommand.RaiseCanExecuteChanged();
                    UpdateCommand.RaiseCanExecuteChanged();
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
            SaveCommand = new RelayCommand(_ => Save(), _ => !IsBusy && !HasErrors);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => !IsBusy && CanDelete);
            RefreshCommand = new RelayCommand(_ => LoadData(), _ => !IsBusy);
            ReloadPageCommand = new RelayCommand(_ => { LoadData(); Clear(); }, _ => !IsBusy);
            ClearCommand = new RelayCommand(_ => Clear(), _ => !IsBusy);
            LocateCommand = new RelayCommand(_ => Locate(), _ => SelectedOrder != null);
            AddCommand = new RelayCommand(_ => AddNewOrder(), _ => !IsBusy);
            UpdateCommand = new RelayCommand(_ => UpdateOrder(), _ => !IsBusy && SelectedOrder != null);
        }

        private void AddNewOrder()
        {
            SelectedOrder = null;
            Clear();
        }

        private void UpdateOrder()
        {
            Save(); // Reuse save logic or implement specific update logic
        }

        private void LoadData()
        {
            try
            {
                IsBusy = true;
                _context = new AppDbContext();

                ProjectOrders = new ObservableCollection<ProjectOrder>(
                    _context.ProjectOrders
                        .Include(p => p.Project)
                        .Include(p => p.Consumer)
                        .ToList());

                Consumers = new ObservableCollection<Consumer>(_context.Consumers.ToList());
                Projects = new ObservableCollection<Project>(_context.Projects.ToList());

                FilterData();
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

        private void FilterData()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ProjectOrders = new ObservableCollection<ProjectOrder>(ProjectOrders);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                ProjectOrders = new ObservableCollection<ProjectOrder>(
                    ProjectOrders.Where(o =>
                        o.Project.Name.ToLower().Contains(searchLower) ||
                        o.ConsumerPhone.Contains(searchLower) ||
                        (o.Notes != null && o.Notes.ToLower().Contains(searchLower)) ||
                        o.FolderSerial.Contains(searchLower))
                );
            }
            OnPropertyChanged(nameof(ProjectOrders));
        }

        private void Save()
        {
            try
            {
                IsBusy = true;

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
                        ConsumerPhone = SelectedConsumerPhone,
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
                    SelectedOrder.ConsumerPhone = SelectedConsumerPhone;
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
            finally
            {
                IsBusy = false;
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

                IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        private void Clear()
        {
            Discount = 0;
            InstallationSystemInfo = string.Empty;
            SoldDate = DateTime.Today;
            Notes = string.Empty;
            SelectedConsumerPhone = string.Empty;
            SelectedProjectId = 0;
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

                    case nameof(InstallationSystemInfo):
                        if (string.IsNullOrWhiteSpace(InstallationSystemInfo))
                            return "Installation info is required";
                        if (InstallationSystemInfo.Length > 500)
                            return "Installation info cannot exceed 500 characters";
                        break;

                    case nameof(SoldDate):
                        if (SoldDate > DateTime.Today)
                            return "Sold date cannot be in the future";
                        break;

                    case nameof(SelectedConsumerPhone):
                        if (string.IsNullOrWhiteSpace(SelectedConsumerPhone))
                            return "Consumer phone is required";
                        break;
                }
                return string.Empty;
            }
        }

        public bool HasErrors
        {
            get
            {
                return !string.IsNullOrEmpty(this[nameof(SelectedProjectId)]) ||
                       !string.IsNullOrEmpty(this[nameof(Discount)]) ||
                       !string.IsNullOrEmpty(this[nameof(InstallationSystemInfo)]) ||
                       !string.IsNullOrEmpty(this[nameof(SoldDate)]) ||
                       !string.IsNullOrEmpty(this[nameof(SelectedConsumerPhone)]);
            }
        }
        #endregion
    }
}