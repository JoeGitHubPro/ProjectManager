using Microsoft.EntityFrameworkCore;
using ProjectManager.Helpers;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ProjectOrderViewModel : BaseViewModel
    {
        private readonly AppDbContext _context;

        public ObservableCollection<ProjectOrder> ProjectOrders { get; set; } = new();
        public ObservableCollection<Consumer> Consumers { get; set; } = new();
        public ObservableCollection<Project> Projects { get; set; } = new();

        public ProjectOrderViewModel()
        {
            _context = new AppDbContext();
            LoadData();
        }

        #region Properties

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set { _discount = value; OnPropertyChanged(); UpdateTotalPrice(); }
        }

        private string _installationSystemInfo;
        public string InstallationSystemInfo
        {
            get => _installationSystemInfo;
            set { _installationSystemInfo = value; OnPropertyChanged(); }
        }

        private DateTime _soldDate = DateTime.Today;
        public DateTime SoldDate
        {
            get => _soldDate;
            set { _soldDate = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private string _selectedConsumerPhone;
        public string SelectedConsumerPhone
        {
            get => _selectedConsumerPhone;
            set { _selectedConsumerPhone = value; OnPropertyChanged(); }
        }

        private int _selectedProjectId;
        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set { _selectedProjectId = value; OnPropertyChanged(); UpdateTotalPrice(); }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterData();
            }
        }

        private ProjectOrder _selectedOrder;
        public ProjectOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                if (_selectedOrder != null)
                {
                    Discount = _selectedOrder.Discount;
                    InstallationSystemInfo = _selectedOrder.InstallationSystemInfo;
                    SoldDate = _selectedOrder.SoldDate;
                    Notes = _selectedOrder.Notes;
                    SelectedConsumerPhone = _selectedOrder.ConsumerPhone;
                    SelectedProjectId = _selectedOrder.ProjectId;
                    TotalPrice = _selectedOrder.Project.Price * (1 - _selectedOrder.Discount / 100);
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddCommand => new RelayCommand(Add);
        public ICommand UpdateCommand => new RelayCommand(Update, () => SelectedOrder != null);
        public ICommand DeleteCommand => new RelayCommand(Delete, () => SelectedOrder != null);
        public ICommand ClearCommand => new RelayCommand(Clear);
        public ICommand LocateCommand => new RelayCommand(Locate);
        public ICommand RefreshCommand => new RelayCommand(LoadData);
        public ICommand ReloadPageCommand => new RelayCommand(ReloadPage);


        #endregion

        #region Methods
        public void ReloadPage()
        {
            LoadData();
            Clear();      // Reset input fields and SelectedOrder
        }

        private void LoadData()
        {
            var orders = _context.ProjectOrders
                .Include(p => p.Project)
                .Include(p => p.Consumer)
                .ToList();

            ProjectOrders.Clear();
            foreach (var order in orders)
            {
                ProjectOrders.Add(order);
            }

            Consumers.Clear();
            foreach (var consumer in _context.Consumers.ToList())
            {
                Consumers.Add(consumer);
            }

            Projects.Clear();
            foreach (var project in _context.Projects.ToList())
            {
                Projects.Add(project);
            }

            OnPropertyChanged(nameof(Projects));
            OnPropertyChanged(nameof(Consumers));
        }


        private void FilterData()
        {
            var allOrders = _context.ProjectOrders
                .Include(p => p.Project)
                .Include(p => p.Consumer)
                .ToList();

            ProjectOrders.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var order in allOrders)
                {
                    ProjectOrders.Add(order);
                }
            }
            else
            {
                var lower = SearchText.ToLower();
                var filtered = allOrders.Where(o =>
                    o.Project.Name.ToLower().Contains(lower) ||
                    o.ConsumerPhone.Contains(lower) ||
                    o.Notes?.ToLower().Contains(lower) == true
                );

                foreach (var order in filtered)
                {
                    ProjectOrders.Add(order);
                }
            }
        }


        private void Add()
        {
            var order = new ProjectOrder
            {
                FolderSerial = FolderHelper.CreateUniqueFolder(),
                Discount = Discount,
                InstallationSystemInfo = InstallationSystemInfo,
                SoldDate = SoldDate,
                Notes = Notes,
                ConsumerPhone = SelectedConsumerPhone,
                ProjectId = SelectedProjectId
            };

            _context.ProjectOrders.Add(order);
            _context.SaveChanges();
            LoadData();
            Clear();
        }

        private void Update()
        {
            if (SelectedOrder == null) return;

            SelectedOrder.Discount = Discount;
            SelectedOrder.InstallationSystemInfo = InstallationSystemInfo;
            SelectedOrder.SoldDate = SoldDate;
            SelectedOrder.Notes = Notes;
            SelectedOrder.ConsumerPhone = SelectedConsumerPhone;
            SelectedOrder.ProjectId = SelectedProjectId;

            _context.ProjectOrders.Update(SelectedOrder);
            _context.SaveChanges();
            LoadData();
            Clear();
        }

        private void Delete()
        {
            if (SelectedOrder == null) return;

            _context.ProjectOrders.Remove(SelectedOrder);
            _context.SaveChanges();
            LoadData();
            Clear();
        }

        private void Clear()
        {
            Discount = 0;
            InstallationSystemInfo = string.Empty;
            SoldDate = DateTime.Today;
            Notes = string.Empty;
            SelectedConsumerPhone = null;
            SelectedProjectId = 0;
            TotalPrice = 0;
            SelectedOrder = null;
        }

        private void Locate()
        {
            if (!string.IsNullOrEmpty(SelectedOrder?.FolderSerial))
            {
                FolderHelper.OpenFolderByName(SelectedOrder.FolderSerial);
            }
        }

        private void UpdateTotalPrice()
        {
            var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
            if (project != null)
            {
                TotalPrice = project.Price * (1 - Discount / 100);
            }
            else
            {
                TotalPrice = 0;
            }
        }

        #endregion
    }
}
