using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class ConsumerViewModel : BaseViewModel
    {
        private AppDbContext _context;
        private string _searchText = string.Empty;
        private Consumer _selectedConsumer;
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

        public ConsumerViewModel()
        {
            InitializeCommands();
            _ = LoadDataAsync(); // Fire-and-forget async initialization
        }

        #region Properties
        public ObservableCollection<Consumer> Consumers { get; private set; } = new();
        public ObservableCollection<Consumer> FilteredConsumers { get; private set; } = new();
        public ObservableCollection<Location> Locations { get; private set; } = new();
        public ObservableCollection<IdentityUser> Sellers { get; private set; } = new();
        public Array ConsumerStatuses => Enum.GetValues(typeof(ConsumerStatus));

        public Location SelectedLocationFilter
        {
            get => selectedLocationFilter;
            set
            {
                selectedLocationFilter = value;
                FilterConsumers();
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

                        // Find and set the selected location
                        SelectedLocation = Locations.FirstOrDefault(l => l.Id == value.LocationId);

                        // Find and set the selected seller
                        SelectedSeller = Sellers.FirstOrDefault(s => s.Id == value.ResponsibleSellerId);
                    }
                }
            }
        }
        private ObservableCollection<Consumer> _selectedConsumers = new ObservableCollection<Consumer>();
        public ObservableCollection<Consumer> SelectedConsumers
        {
            get => _selectedConsumers;
            set
            {
                _selectedConsumers = value;
                OnPropertyChanged();

                // You can trigger any logic that depends on the selection here
                OnPropertyChanged(nameof(CanDelete));
                IsFormVisible = _selectedConsumers.Any();

                // Example: if you want to use the first selected item to populate form fields
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

                    // Set selected location and seller from the first consumer
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
            get => selectedSellerFilter;
            set
            {
                selectedSellerFilter = value;
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
            set
            {
                _consumerStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isFormVisible = false;
        private Location selectedLocationFilter;
        private IdentityUser selectedSellerFilter;
        private string rangeStart;
        private string rangeEnd;

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
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
        public string RangeStart { get => rangeStart; set { SetField(ref rangeStart, value); FilterConsumers(); } }
        public string RangeEnd { get => rangeEnd; set { SetField(ref rangeEnd, value); FilterConsumers(); } }
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

        private async Task AssignConsumerAsync()
        {
            var dialog = new AssignConsumers();

            if (dialog.ShowDialog() == true)
            {
                string? sellerId = dialog.SelectedUserId;

                if (!string.IsNullOrWhiteSpace(sellerId) && SelectedConsumers?.Any() == true)
                {
                    var consumers = SelectedConsumers.ToList();

                    foreach (var consumer in consumers)
                    {
                        consumer.ResponsibleSellerId = sellerId;
                    }

                    _context.Consumers.UpdateRange(consumers);
                    await _context.SaveChangesAsync();
                    await ShowInfoAsync($"Consumers assigned successfully");
                    await LoadDataAsync();
                }
            }
        }


        #endregion

        #region Async Methods
        private async Task LoadDataAsync()
        {
            try
            {
                ClearFields();
                _context = new AppDbContext();

                var consumers = await _context.Consumers
                    .Include(c => c.Location)
                    .Include(c => c.ResponsibleSeller)
                    .ToListAsync();

                var locations = await _context.Set<Location>().ToListAsync();
                var sellers = await _context.Set<IdentityUser>().ToListAsync();

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

        private async Task<List<Consumer>> ValidateAndImportExcelDataAsync(string filePath)
        {
            // Step 1: Read Excel data (offload to background thread)
            var rawConsumers = await Task.Run(() => ReaderHelper.ReadConsumersFromExcelAsync(filePath));

            // Step 2: Validate Excel data
            var validConsumers = ValidateExcelData(rawConsumers);

            // Step 3: Filter against existing database records
            return await FilterNewConsumersAgainstDatabaseAsync(validConsumers);
        }

        private List<Consumer> ValidateExcelData(List<Consumer> consumers)
        {
            if (consumers == null || consumers.Count == 0)
                return new List<Consumer>();

            // Basic validation - remove empty names
            var validNameConsumers = consumers
                .Where(c => !string.IsNullOrWhiteSpace(c?.Name))
                .ToList();

            // Ensure unique names (case insensitive)
            validNameConsumers = validNameConsumers
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Split into phone-based groups
            var withPhones = validNameConsumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
                .ToList();

            var withoutPhones = validNameConsumers
                .Where(c => string.IsNullOrWhiteSpace(c.Phone))
                .ToList();

            // Ensure unique phones
            var uniquePhones = withPhones
                .GroupBy(c => c.Phone.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Split into CRN-based groups
            var withCRNs = uniquePhones
                .Where(c => !string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .ToList();

            var withoutCRNs = uniquePhones
                .Where(c => string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .ToList();

            // Ensure unique CRNs
            var uniqueCRNs = withCRNs
                .GroupBy(c => c.CommercialRegisterNo.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Set default location and status if needed
            foreach (var consumer in uniqueCRNs.Concat(withoutCRNs).Concat(withoutPhones))
            {
                // Set default location if not specified
                if (consumer.LocationId == 0 && Locations.Any())
                {
                    consumer.LocationId = Locations.First().Id;
                }

                // Set default status
                consumer.Status = ConsumerStatus.Default;
            }

            // Combine all valid entries
            return uniqueCRNs
                .Concat(withoutCRNs)
                .Concat(withoutPhones)
                .Distinct()
                .ToList();
        }

        private async Task<List<Consumer>> FilterNewConsumersAgainstDatabaseAsync(List<Consumer> validatedConsumers)
        {
            if (validatedConsumers == null || validatedConsumers.Count == 0)
                return new List<Consumer>();

            // Fetch existing values from DB
            var existingData = await _context.Consumers
                .Select(c => new
                {
                    Name = c.Name.Trim(),
                    Phone = c.Phone != null ? c.Phone.Trim() : null,
                    CRN = c.CommercialRegisterNo != null ? c.CommercialRegisterNo.Trim() : null
                })
                .ToListAsync();

            var existingNames = existingData.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingPhones = existingData.Where(x => x.Phone != null).Select(x => x.Phone).ToHashSet();
            var existingCRNs = existingData.Where(x => x.CRN != null).Select(x => x.CRN).ToHashSet();

            // Filter consumers that don't exist in database
            return validatedConsumers
                .Where(c =>
                {
                    var name = c.Name?.Trim();
                    var phone = c.Phone?.Trim();
                    var crn = c.CommercialRegisterNo?.Trim();

                    // Name must be unique (case insensitive)
                    bool isNameNew = !string.IsNullOrWhiteSpace(name) &&
                                    !existingNames.Contains(name);

                    // Phone must be either null or unique
                    bool isPhoneNew = string.IsNullOrWhiteSpace(phone) ||
                                    !existingPhones.Contains(phone);

                    // CRN must be either null or unique
                    bool isCrnNew = string.IsNullOrWhiteSpace(crn) ||
                                   !existingCRNs.Contains(crn);

                    return isNameNew && isPhoneNew && isCrnNew;
                })
                .ToList();
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
                    var newConsumers = await ValidateAndImportExcelDataAsync(openFileDialog.FileName);

                    if (newConsumers.Any())
                    {
                        await _context.Consumers.AddRangeAsync(newConsumers);
                        await _context.SaveChangesAsync();
                        SelectedConsumers = new ObservableCollection<Consumer>(newConsumers);
                        await AssignConsumerAsync();
                        await ShowInfoAsync($"{newConsumers.Count} consumers imported successfully");
                        await LoadDataAsync();

                    }
                    else
                    {
                        await ShowWarningAsync("No valid new consumers found in the Excel file!");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync($"Error importing Excel data: {ex.Message}");
                }
            }
        }
        private async Task SaveConsumerAsync()
        {
            if (HasErrors)
            {
                await ShowWarningAsync("Please fix validation errors before saving.");
                return;
            }

            if (LocationId == 0)
            {
                await ShowWarningAsync("Please select a location.");
                return;
            }

            bool result = _isNewRecord ?
                await SaveNewConsumerAsync() :
                await UpdateExistingConsumerAsync();

            if (result)
            {
                FilterConsumers();
                await ShowInfoAsync("Consumer saved successfully.");
                await LoadDataAsync();


            }
        }

        private async Task<bool> SaveNewConsumerAsync()
        {
            if (!string.IsNullOrWhiteSpace(Phone) && !string.IsNullOrWhiteSpace(CommercialRegisterNo))
            {
                if (await _context.Consumers.AnyAsync(c =>
                    c.Phone == Phone || c.CommercialRegisterNo == CommercialRegisterNo))
                {
                    await ShowWarningAsync("A consumer with this phone number or commercial register already exists.");
                    return false;
                }
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

            await _context.Consumers.AddAsync(consumer);
            await _context.SaveChangesAsync();

            // Reload the consumer to get navigation properties
            consumer = await _context.Consumers
                .Include(c => c.Location)
                .Include(c => c.ResponsibleSeller)
                .FirstOrDefaultAsync(c => c.Id == consumer.Id);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Consumers.Add(consumer);
                FilteredConsumers.Add(consumer);
            });

            return true;
        }

        private async Task<bool> UpdateExistingConsumerAsync()
        {
            if (SelectedConsumer == null || _context == null)
                return false;

            SelectedConsumer.Phone = Phone;
            SelectedConsumer.CommercialRegisterNo = CommercialRegisterNo;
            SelectedConsumer.Name = Name;
            SelectedConsumer.Description = Description;
            SelectedConsumer.LocationId = LocationId;
            SelectedConsumer.LocationMap = LocationMap;
            SelectedConsumer.Status = ConsumerStatus;
            SelectedConsumer.ResponsibleSellerId = ResponsibleSellerId;
            SelectedConsumer.Whatsapp = Whatsapp;
            SelectedConsumer.Facebook = Facebook;
            SelectedConsumer.TikTok = TikTok;
            SelectedConsumer.X = X;
            SelectedConsumer.Instagram = Instagram;
            SelectedConsumer.Site = Site;
            SelectedConsumer.Notes = Notes;

            _context.Consumers.Update(SelectedConsumer);
            await _context.SaveChangesAsync();

            // Reload the consumer to get updated navigation properties
            var updatedConsumer = await _context.Consumers
                .Include(c => c.Location)
                .Include(c => c.ResponsibleSeller)
                .FirstOrDefaultAsync(c => c.Id == SelectedConsumer.Id);

            // Update the collections in UI
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var index = Consumers.IndexOf(SelectedConsumer);
                if (index >= 0)
                {
                    Consumers[index] = updatedConsumer;
                }

                index = FilteredConsumers.IndexOf(SelectedConsumer);
                if (index >= 0)
                {
                    FilteredConsumers[index] = updatedConsumer;
                }

                SelectedConsumer = updatedConsumer;
            });

            return true;
        }

        private async Task DeleteConsumerAsync()
        {
            if (SelectedConsumer == null || _context == null) return;

            var confirmed = await ShowConfirmationDialogAsync(
                "Are you sure you want to delete this consumer? This action cannot be undone.");

            if (!confirmed) return;

            try
            {
                _context.Consumers.Remove(SelectedConsumer);
                await _context.SaveChangesAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Consumers.Remove(SelectedConsumer);
                    FilteredConsumers.Remove(SelectedConsumer);
                    ClearFields();
                });

                await ShowInfoAsync("Consumer deleted successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error deleting consumer: {ex.Message}");
            }
        }
        #endregion

        #region Synchronous Methods
        private void FilterConsumers()
        {
            IEnumerable<Consumer> filtered = Consumers;

            // Apply Search Text Filter
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

            // Apply Location Filter
            if (selectedLocationFilter != null)
            {
                filtered = filtered.Where(c => c.LocationId == selectedLocationFilter.Id);
            }

            // Apply Responsible Seller Filter
            if (selectedSellerFilter != null)
            {
                filtered = filtered.Where(c => c.ResponsibleSellerId == selectedSellerFilter.Id);
            }

            // Apply Status Filter
            if (SelectedStatus != null)
            {
                filtered = filtered.Where(c => c.Status == SelectedStatus);
            }

            // Apply Range Filter
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

            // Set defaults for new consumer
            ConsumerStatus = ConsumerStatus.Default;

            // Set default location if available
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

        private readonly Dictionary<string, string> _errorCollection = new();
        public Dictionary<string, string> ErrorCollection => _errorCollection;

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

        public bool HasErrors => _errorCollection.Values.Any(error => !string.IsNullOrWhiteSpace(error));
        #endregion

        #region Helper Methods
        private void SetField(ref string field, string value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        private void SetField(ref int field, int value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
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
        private async Task ShowErrorAsync(string message) =>
            await Application.Current.Dispatcher.InvokeAsync(() => new ErrorDialog("Error", message));

        private async Task ShowWarningAsync(string message) =>
            await Application.Current.Dispatcher.InvokeAsync(() => new WarningDialog(message));

        private async Task ShowInfoAsync(string message) =>
            await Application.Current.Dispatcher.InvokeAsync(() => new InformationDialog(message));
        #endregion
    }
}