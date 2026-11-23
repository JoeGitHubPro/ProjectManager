using Microsoft.AspNetCore.Identity;
using Microsoft.Win32;
using ProjectManager.Constants;
using ProjectManager.Models;
using ProjectManager.Service;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ConsumerViewModel : BaseViewModel
    {
        private readonly IConsumerService _consumerService;
        private string _searchText = string.Empty;
        private Consumer _selectedConsumer;
        private ObservableCollection<Consumer> _selectedConsumers = new ObservableCollection<Consumer>();
        private string _phone = string.Empty;
        private string _commercialRegisterNo = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _locationMap = string.Empty;
        private string _whatsapp = string.Empty;
        private string _facebook = string.Empty;
        private string _tikTok = string.Empty;
        private string _x = string.Empty;
        private string _instagram = string.Empty;
        private string _site = string.Empty;
        private string _notes = string.Empty;
        private Location _selectedLocation;
        private int _locationId;
        private IdentityUser _selectedSeller;
        private ConsumerStatus? _selectedStatus = null;
        private string _responsibleSellerId;
        private ConsumerStatus _consumerStatus = ConsumerStatus.Default;
        private bool _isNewRecord = true;
        private Location _selectedLocationFilter;
        private IdentityUser _selectedSellerFilter;
        private string _rangeStart;
        private string _rangeEnd;
        private bool _isFormVisible = false;
        private readonly Dictionary<string, string> _errorCollection = new();

        public ConsumerViewModel(IConsumerService consumerService)
        {
            _consumerService = consumerService;
            InitializeCommands();
            _ = LoadDataAsync();
        }

        #region Properties
        public ObservableCollection<Consumer> Consumers { get; private set; } = new();
        public ObservableCollection<Consumer> FilteredConsumers { get; private set; } = new();
        public ObservableCollection<Location> Locations { get; private set; } = new();
        public ObservableCollection<IdentityUser> Sellers { get; private set; } = new();
        public Array ConsumerStatuses => Enum.GetValues(typeof(ConsumerStatus));

        public Location SelectedLocationFilter
        {
            get => _selectedLocationFilter;
            set
            {
                _selectedLocationFilter = value;
                FilterConsumers();
                OnPropertyChanged();
            }
        }

        public Consumer SelectedConsumer
        {
            get => _selectedConsumer;
            set
            {
                if (_selectedConsumer != value)
                {
                    _selectedConsumer = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanDelete));

                    if (value != null)
                    {
                        _isNewRecord = false;
                        Phone = value.Phone;
                        CommercialRegisterNo = value.CommercialRegisterNo;
                        Name = value.Name;
                        Description = value.Description;
                        LocationId = value.LocationId;
                        LocationMap = value.LocationMap;
                        ConsumerStatus = value.Status;
                        ResponsibleSellerId = value.ResponsibleSellerId;
                        Whatsapp = value.Whatsapp;
                        Facebook = value.Facebook;
                        TikTok = value.TikTok;
                        X = value.X;
                        Instagram = value.Instagram;
                        Site = value.Site;
                        Notes = value.Notes;

                        SelectedLocation = Locations.FirstOrDefault(l => l.Id == value.LocationId);
                        SelectedSeller = Sellers.FirstOrDefault(s => s.Id == value.ResponsibleSellerId);
                    }
                }
            }
        }

        public ObservableCollection<Consumer> SelectedConsumers
        {
            get => _selectedConsumers;
            set
            {
                _selectedConsumers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));
                IsFormVisible = _selectedConsumers.Any();

                var first = _selectedConsumers.FirstOrDefault();
                if (first != null)
                {
                    _isNewRecord = false;
                    Phone = first.Phone;
                    CommercialRegisterNo = first.CommercialRegisterNo;
                    Name = first.Name;
                    Description = first.Description;
                    LocationId = first.LocationId;
                    LocationMap = first.LocationMap;
                    ConsumerStatus = first.Status;
                    ResponsibleSellerId = first.ResponsibleSellerId;
                    Whatsapp = first.Whatsapp;
                    Facebook = first.Facebook;
                    TikTok = first.TikTok;
                    X = first.X;
                    Instagram = first.Instagram;
                    Site = first.Site;
                    Notes = first.Notes;

                    SelectedLocation = Locations.FirstOrDefault(l => l.Id == first.LocationId);
                    SelectedSeller = Sellers.FirstOrDefault(s => s.Id == first.ResponsibleSellerId);
                }
            }
        }

        public Location SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                _selectedLocation = value;
                if (value != null)
                {
                    LocationId = value.Id;
                }
                OnPropertyChanged();
            }
        }

        public IdentityUser SelectedSeller
        {
            get => _selectedSeller;
            set
            {
                _selectedSeller = value;
                if (value != null)
                {
                    ResponsibleSellerId = value.Id;
                }
                OnPropertyChanged();
            }
        }

        public IdentityUser SelectedSellerFilter
        {
            get => _selectedSellerFilter;
            set
            {
                _selectedSellerFilter = value;
                FilterConsumers();
                OnPropertyChanged();
            }
        }

        public ConsumerStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (_selectedStatus != value)
                {
                    _selectedStatus = value;
                    FilterConsumers();
                    OnPropertyChanged();
                }
            }
        }

        public ConsumerStatus ConsumerStatus
        {
            get => _consumerStatus;
            set => SetField(ref _consumerStatus, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetField(ref _isFormVisible, value);
        }

        public string Name
        {
            get => _name;
            set => ValidateAndNotify(ref _name, value, nameof(Name));
        }

        public string Description { get => _description; set => SetField(ref _description, value); }
        public int LocationId { get => _locationId; set => SetField(ref _locationId, value); }
        public string LocationMap { get => _locationMap; set => SetField(ref _locationMap, value); }
        public string ResponsibleSellerId { get => _responsibleSellerId; set => SetField(ref _responsibleSellerId, value); }
        public string Whatsapp { get => string.IsNullOrEmpty(_whatsapp) ? Phone : _whatsapp; set => SetField(ref _whatsapp, value); }
        public string Facebook { get => _facebook; set => SetField(ref _facebook, value); }
        public string TikTok { get => _tikTok; set => SetField(ref _tikTok, value); }
        public string X { get => _x; set => SetField(ref _x, value); }
        public string Instagram { get => _instagram; set => SetField(ref _instagram, value); }
        public string Site { get => _site; set => SetField(ref _site, value); }
        public string Notes { get => _notes; set => SetField(ref _notes, value); }
        public string Phone { get => _phone; set => SetField(ref _phone, value); }
        public string RangeStart { get => _rangeStart; set { SetField(ref _rangeStart, value); } }
        public string RangeEnd { get => _rangeEnd; set { SetField(ref _rangeEnd, value); FilterConsumers(); } }

        public string CommercialRegisterNo
        {
            get => _commercialRegisterNo;
            set => ValidateAndNotify(ref _commercialRegisterNo, value, nameof(CommercialRegisterNo));
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterConsumers();
            }
        }

        public bool CanDelete => SelectedConsumer != null;
        public Dictionary<string, string> ErrorCollection => _errorCollection;
        public bool HasErrors => _errorCollection.Values.Any(error => !string.IsNullOrWhiteSpace(error));
        #endregion

        #region Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand ApplyRangeFilterCommand { get; private set; }
        public ICommand AddNewCommand { get; private set; }
        public ICommand AssignCommand { get; private set; }
        public ICommand ReadExcelCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveConsumerAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteConsumerAsync);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchCommand = new RelayCommand(FilterConsumers);
            ApplyRangeFilterCommand = new RelayCommand(FilterConsumers);
            AddNewCommand = new RelayCommand(AddNewConsumer);
            AssignCommand = new AsyncRelayCommand(AssignConsumerAsync);
            ReadExcelCommand = new AsyncRelayCommand(ReadExcelAsync);
            EditCommand = new RelayCommand(EditConsumer);
            ClearCommand = new RelayCommand(ClearFields);
        }
        #endregion

        #region Service Methods
        public async Task LoadDataAsync()
        {
            try
            {
                ClearFields();

                var consumers = await _consumerService.GetAllConsumersAsync();
                var locations = await _consumerService.GetAllLocationsAsync();
                var sellers = await _consumerService.GetAllSellersAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Consumers = new ObservableCollection<Consumer>(consumers);
                    FilteredConsumers = new ObservableCollection<Consumer>(consumers);
                    Locations = new ObservableCollection<Location>(locations);
                    Sellers = new ObservableCollection<IdentityUser>(sellers);

                    OnPropertyChanged(nameof(Consumers));
                    OnPropertyChanged(nameof(FilteredConsumers));
                    OnPropertyChanged(nameof(Locations));
                    OnPropertyChanged(nameof(Sellers));
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error loading data: {ex.Message}");
            }
        }

        private async Task SaveConsumerAsync()
        {
            if (HasErrors)
            {
                await ShowWarningDialogAsync("Please fix validation errors before saving.");
                return;
            }

            if (LocationId == 0)
            {
                await ShowWarningDialogAsync("Please select a location.");
                return;
            }

            var consumer = new Consumer
            {
                Phone = Phone,
                CommercialRegisterNo = CommercialRegisterNo,
                Name = Name,
                Description = Description,
                LocationId = LocationId,
                LocationMap = LocationMap,
                Status = ConsumerStatus,
                ResponsibleSellerId = ResponsibleSellerId,
                Whatsapp = Whatsapp,
                Facebook = Facebook,
                TikTok = TikTok,
                X = X,
                Instagram = Instagram,
                Site = Site,
                Notes = Notes
            };

            try
            {
                Consumer result;
                if (_isNewRecord)
                {
                    result = await _consumerService.CreateConsumerAsync(consumer);
                }
                else
                {
                    consumer.Id = SelectedConsumer.Id;
                    result = await _consumerService.UpdateConsumerAsync(consumer);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (_isNewRecord)
                    {
                        Consumers.Add(result);
                        FilteredConsumers.Add(result);
                    }
                    else
                    {
                        var index = Consumers.IndexOf(SelectedConsumer);
                        if (index >= 0) Consumers[index] = result;

                        index = FilteredConsumers.IndexOf(SelectedConsumer);
                        if (index >= 0) FilteredConsumers[index] = result;

                        SelectedConsumer = result;
                    }
                });

                await ShowInformationDialogAsync("Consumer saved successfully.");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error saving consumer: {ex.Message}");
            }
        }

        private async Task DeleteConsumerAsync()
        {
            if (SelectedConsumer == null) return;

            var confirmed = await ShowConfirmationDialogAsync(
                "Are you sure you want to delete this consumer? This action cannot be undone.");

            if (!confirmed) return;

            try
            {
                await _consumerService.DeleteConsumerAsync(SelectedConsumer.Id);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Consumers.Remove(SelectedConsumer);
                    FilteredConsumers.Remove(SelectedConsumer);
                    ClearFields();
                });

                await ShowInformationDialogAsync("Consumer deleted successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error deleting consumer: {ex.Message}");
            }
        }

        private async Task AssignConsumerAsync()
        {
            var dialog = new AssignConsumers();
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.SelectedUserId))
            {
                try
                {
                    var consumerIds = SelectedConsumers.Select(c => c.Id).ToList();
                    await _consumerService.AssignConsumersAsync(consumerIds, dialog.SelectedUserId);

                    await ShowInformationDialogAsync("Consumers assigned successfully");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync($"Error assigning consumers: {ex.Message}");
                }
            }
        }

        private async Task ReadExcelAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var newConsumers = await _consumerService.ImportConsumersFromExcelAsync(openFileDialog.FileName);

                    if (newConsumers.Any())
                    {
                        SelectedConsumers = new ObservableCollection<Consumer>(newConsumers);
                        await AssignConsumerAsync();
                        await ShowInformationDialogAsync($"{newConsumers.Count} consumers imported successfully");
                        await LoadDataAsync();
                    }
                    else
                    {
                        await ShowWarningDialogAsync("No valid new consumers found in the Excel file!");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync($"Error importing Excel data: {ex.Message}");
                }
            }
        }
        #endregion

        #region UI Methods
        private void FilterConsumers()
        {
            IEnumerable<Consumer> filtered = Consumers;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(search)) ||
                    (c.Location != null && !string.IsNullOrEmpty(c.Location.Name) && c.Location.Name.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.LocationMap) && c.LocationMap.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Whatsapp) && c.Whatsapp.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Facebook) && c.Facebook.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.TikTok) && c.TikTok.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.X) && c.X.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Instagram) && c.Instagram.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Site) && c.Site.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Notes) && c.Notes.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.CommercialRegisterNo) && c.CommercialRegisterNo.ToLower().Contains(search)) ||
                    (c.ResponsibleSeller != null && !string.IsNullOrEmpty(c.ResponsibleSeller.UserName) && c.ResponsibleSeller.UserName.ToLower().Contains(search))
                );
            }

            if (_selectedLocationFilter != null)
            {
                filtered = filtered.Where(c => c.LocationId == _selectedLocationFilter.Id);
            }

            if (_selectedSellerFilter != null)
            {
                filtered = filtered.Where(c => c.ResponsibleSellerId == _selectedSellerFilter.Id);
            }

            if (SelectedStatus != null)
            {
                filtered = filtered.Where(c => c.Status == SelectedStatus);
            }

            if (int.TryParse(RangeStart, out int start) && int.TryParse(RangeEnd, out int end) && (start >= 0 || end >= 0))
            {
                filtered = filtered.Where(c => c.Id >= start && c.Id <= end);
            }

            FilteredConsumers = new ObservableCollection<Consumer>(filtered);
            OnPropertyChanged(nameof(FilteredConsumers));
        }

        private void AddNewConsumer()
        {
            ClearFields();
            _isNewRecord = true;
            ConsumerStatus = ConsumerStatus.Default;

            if (Locations.Count > 0)
            {
                SelectedLocation = Locations.First();
            }

            IsFormVisible = true;
        }

        private void EditConsumer()
        {
            IsFormVisible = true;
        }

        private void ClearFields()
        {
            SelectedConsumer = null;
            Phone = CommercialRegisterNo = Name = Description = LocationMap = Whatsapp =
                Facebook = TikTok = X = Instagram = Site = Notes = string.Empty;

            RangeStart = RangeEnd = SearchText = string.Empty;

            LocationId = 0;
            SelectedLocation = null;
            SelectedSeller = null;
            SelectedStatus = null;
            ResponsibleSellerId = null;
            SelectedLocationFilter = null;
            SelectedSellerFilter = null;
            ConsumerStatus = ConsumerStatus.Default;

            _isNewRecord = true;
            IsFormVisible = false;
        }
        #endregion

        #region Validation
        public string this[string propertyName]
        {
            get
            {
                return propertyName switch
                {
                    nameof(Name) => string.IsNullOrWhiteSpace(Name)
                        ? "Name is required"
                        : Name.Length < 3
                            ? "Name cannot be less than 3 characters"
                            : string.Empty,

                    nameof(CommercialRegisterNo) => !string.IsNullOrWhiteSpace(CommercialRegisterNo) && CommercialRegisterNo.Length > 50
                        ? "Commercial register number cannot exceed 50 characters"
                        : string.Empty,

                    _ => string.Empty
                };
            }
        }

        private void ValidateProperty(string propertyName)
        {
            string error = this[propertyName];
            _errorCollection[propertyName] = error;
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void ValidateAndNotify(ref string backingField, string value, string propertyName)
        {
            backingField = value;
            OnPropertyChanged(propertyName);
            ValidateProperty(propertyName);
        }
        #endregion
    }
}