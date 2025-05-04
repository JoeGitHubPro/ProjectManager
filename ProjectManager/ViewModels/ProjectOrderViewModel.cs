using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Models;
using ProjectManager.ViewModels.CapitalManagement;
using ProjectManager.Views.Dialogs;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ProjectOrderViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<string, List<string>> _errors = new();

        public ObservableCollection<ProjectOrder> ProjectOrders { get; } = new();
        public ObservableCollection<Consumer> Consumers { get; } = new();
        public ObservableCollection<Project> Projects { get; } = new();

        public ProjectOrderViewModel()
        {
            _context = new AppDbContext();
            InitializeCommands();
            _ = LoadDataAsync(); // Fire-and-forget async initialization
        }

        #region Properties

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                SetProperty(ref _discount, value);
                ValidateProperty(value);
                UpdateTotalPrice();
            }
        }

        private decimal _commission;
        public decimal Commission
        {
            get => _commission;
            set
            {
                SetProperty(ref _commission, value);
                ValidateProperty(value);
                UpdateTotalPrice();
            }
        }

        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        private string _installationSystemInfo = string.Empty;
        public string InstallationSystemInfo
        {
            get => _installationSystemInfo;
            set => SetProperty(ref _installationSystemInfo, value);
        }

        private DateTime _soldDate = DateTime.Today;
        public DateTime SoldDate
        {
            get => _soldDate;
            set
            {
                SetProperty(ref _soldDate, value);
                ValidateProperty(value);
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private int _selectedConsumerId;
        public int SelectedConsumerId
        {
            get => _selectedConsumerId;
            set
            {
                SetProperty(ref _selectedConsumerId, value);
                ValidateProperty(value);
            }
        }

        private int _selectedProjectId;
        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set
            {
                SetProperty(ref _selectedProjectId, value);
                ValidateProperty(value);
                UpdateTotalPrice();
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = FilterDataAsync();
            }
        }

        private ProjectOrder _selectedOrder;
        public ProjectOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    OnPropertyChanged(nameof(CanDelete));

                    if (value != null)
                    {
                        Discount = value.Discount;
                        Commission = value.Commission;
                        InstallationSystemInfo = value.InstallationSystemInfo;
                        SoldDate = value.SoldDate;
                        Notes = value.Notes;
                        SelectedConsumerId = value.ConsumerId;
                        SelectedProjectId = value.ProjectId;
                    }
                }
            }
        }

        public bool CanDelete => SelectedOrder != null;

        #endregion

        #region Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ReloadPageCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand LocateCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        #endregion

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CanDelete);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            ReloadPageCommand = new AsyncRelayCommand(async () => { await LoadDataAsync(); Clear(); });
            ClearCommand = new RelayCommand(Clear);
            LocateCommand = new AsyncRelayCommand(Locate);
            AddCommand = new RelayCommand(AddNewOrder);
            UpdateCommand = new RelayCommand(UpdateOrder);
        }

        private void AddNewOrder() { SelectedOrder = null; Clear(); IsFormVisible = true; }
        private void UpdateOrder() => IsFormVisible = true;

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProjectOrders.Clear();
                    Consumers.Clear();
                    Projects.Clear();
                });

                var orders = await _context.ProjectOrders
                    .Include(p => p.Project)
                    .Include(p => p.Consumer)
                    .ToListAsync();

                var consumers = await _context.Consumers.ToListAsync();
                var projects = await _context.Projects
                    .Include(p => p.ProjectCategory)
                    .ToListAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var o in orders) ProjectOrders.Add(o);
                    foreach (var c in consumers) Consumers.Add(c);
                    foreach (var p in projects) Projects.Add(p);
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading data", ex.Message);
            }
        }

        private async Task FilterDataAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadDataAsync();
                    return;
                }

                var searchLower = SearchText.ToLower();
                var filtered = await _context.ProjectOrders
                    .Include(p => p.Project)
                    .Include(p => p.Consumer)
                    .Where(o =>
                        o.Project.Name.ToLower().Contains(searchLower) ||
                        o.Consumer.Phone.Contains(searchLower) ||
                        (o.Notes != null && o.Notes.ToLower().Contains(searchLower)) ||
                        o.FolderSerial.Contains(searchLower))
                    .ToListAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProjectOrders.Clear();
                    foreach (var o in filtered) ProjectOrders.Add(o);
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error filtering data", ex.Message);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (HasErrors)
                {
                    await ShowWarningAsync("Validation Error", "Please fix validation errors.");
                    return;
                }

                var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
                if (project == null)
                {
                    await ShowErrorAsync("Error", "Please select a valid project.");
                    return;
                }

                if (SelectedOrder == null)
                {
                    var order = new ProjectOrder
                    {
                        FolderSerial = await FolderHelper.CreateUniqueFolder(),
                        Discount = Discount,
                        Commission = Commission,
                        InstallationSystemInfo = InstallationSystemInfo,
                        SoldDate = SoldDate,
                        Notes = Notes,
                        ConsumerId = SelectedConsumerId,
                        ProjectId = SelectedProjectId,
                        TotalPrice = project.Price * (1 - Discount / 100 - Commission / 100)
                    };

                    // Save income record
                    var balanceVm = new BusinessBalanceViewModel();

                    await balanceVm.SaveIncomeAsync(
                        order.TotalPrice,
                        $"Selling {project.ProjectCategory?.Name} {project.Name}");

                    await _context.ProjectOrders.AddAsync(order);
                }
                else
                {
                    SelectedOrder.Discount = Discount;
                    SelectedOrder.Commission = Commission;
                    SelectedOrder.InstallationSystemInfo = InstallationSystemInfo;
                    SelectedOrder.SoldDate = SoldDate;
                    SelectedOrder.Notes = Notes;
                    SelectedOrder.ConsumerId = SelectedConsumerId;
                    SelectedOrder.ProjectId = SelectedProjectId;
                    SelectedOrder.TotalPrice = project.Price * (1 - Discount / 100 - Commission / 100);
                }

                await _context.SaveChangesAsync();
                await LoadDataAsync();
                Clear();
                await ShowInfoAsync("Success", "Order saved successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving order", ex.Message);
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (SelectedOrder == null) return;

                var confirmed = await ShowConfirmationDialogAsync(
                    "Confirm Delete",
                    "Are you sure you want to delete this order?");

                if (!confirmed) return;

                FolderHelper.DeleteFolder(SelectedOrder.FolderSerial);
                _context.ProjectOrders.Remove(SelectedOrder);
                await _context.SaveChangesAsync();

                await LoadDataAsync();
                Clear();
                await ShowInfoAsync("Success", "Order deleted successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error deleting order", ex.Message);
            }
        }

        private void Clear()
        {
            Discount = 0;
            Commission = 0;
            InstallationSystemInfo = string.Empty;
            SoldDate = DateTime.Today;
            Notes = string.Empty;
            SelectedConsumerId = 0;
            SelectedProjectId = 0;
            SelectedOrder = null;
            IsFormVisible = false;
        }

        private async Task Locate()
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedOrder?.FolderSerial))
                {
                    await Task.Run(() => FolderHelper.OpenFolderByName(SelectedOrder.FolderSerial));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error opening folder", ex.Message);
            }
        }

        private void UpdateTotalPrice()
        {
            var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
            TotalPrice = project?.Price * (1 - Discount / 100 - Commission / 100) ?? 0;
        }

        #region Validation (INotifyDataErrorInfo)

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            List<string> errors = new();

            switch (propertyName)
            {
                case nameof(Discount):
                    if ((decimal)value < 0 || (decimal)value > 100)
                        errors.Add("Discount must be between 0 and 100.");
                    break;

                case nameof(Commission):
                    if ((decimal)value < 0)
                        errors.Add("Commission cannot be negative.");
                    break;

                case nameof(SoldDate):
                    if ((DateTime)value > DateTime.Today)
                        errors.Add("Sold date cannot be in the future.");
                    break;

                case nameof(SelectedProjectId):
                    if ((int)value <= 0)
                        errors.Add("Please select a project.");
                    break;

                case nameof(SelectedConsumerId):
                    if ((int)value <= 0)
                        errors.Add("Consumer is required.");
                    break;
            }

            if (errors.Any())
                _errors[propertyName] = errors;
            else
                _errors.Remove(propertyName);

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Dialog Helpers
        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }

        private async Task ShowWarningAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new WarningDialog($"{title}\t{message}");
            });
        }

        private async Task ShowInfoAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new InformationDialog($"{title}\t{message}");
            });
        }

        private async Task<bool> ShowConfirmationDialogAsync(string title, string message)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog($"{title}\t{message}");
                dialog.ShowDialog();
                return dialog.IsConfirmed;
            });
        }
        #endregion
    }
}