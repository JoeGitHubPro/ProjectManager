using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
        private string _location = string.Empty;
        private string _whatsapp = string.Empty;
        private string _facebook = string.Empty;
        private string _tikTok = string.Empty;
        private string _x = string.Empty;
        private string _instagram = string.Empty;
        private string _site = string.Empty;
        private string _notes = string.Empty;
        private bool _isNewRecord = true;

        public ConsumerViewModel()
        {
            InitializeCommands();
            _ = LoadDataAsync(); // Fire-and-forget async initialization
        }

        #region Properties
        public ObservableCollection<Consumer> Consumers { get; private set; } = new();
        public ObservableCollection<Consumer> FilteredConsumers { get; private set; } = new();

        public Consumer SelectedConsumer
        {
            get => _selectedConsumer;
            set
            {
                _selectedConsumer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));

                if (value != null)
                {
                    _isNewRecord = false;
                    IsFormVisible = true;

                    Phone = value.Phone;
                    CommercialRegisterNo = value.CommercialRegisterNo;
                    Name = value.Name;
                    Description = value.Description;
                    Location = value.Location;
                    Whatsapp = value.Whatsapp;
                    Facebook = value.Facebook;
                    TikTok = value.TikTok;
                    X = value.X;
                    Instagram = value.Instagram;
                    Site = value.Site;
                    Notes = value.Notes;
                }
            }
        }

        private bool _isFormVisible = false;
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
        public string Location { get => _location; set => SetField(ref _location, value); }
        public string Whatsapp { get => string.IsNullOrEmpty(_whatsapp) ? Phone : _whatsapp; set => SetField(ref _whatsapp, value); }
        public string Facebook { get => _facebook; set => SetField(ref _facebook, value); }
        public string TikTok { get => _tikTok; set => SetField(ref _tikTok, value); }
        public string X { get => _x; set => SetField(ref _x, value); }
        public string Instagram { get => _instagram; set => SetField(ref _instagram, value); }
        public string Site { get => _site; set => SetField(ref _site, value); }
        public string Notes { get => _notes; set => SetField(ref _notes, value); }
        public string Phone { get => _phone; set => SetField(ref _phone, value); }

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
        public ICommand AddNewCommand { get; private set; }
        public ICommand ReadExcelCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveConsumerAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteConsumerAsync);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchCommand = new RelayCommand(FilterConsumers);
            AddNewCommand = new RelayCommand(AddNewConsumer);
            ReadExcelCommand = new AsyncRelayCommand(ReadExcelAsync);
            EditCommand = new RelayCommand(EditConsumer);
            ClearCommand = new RelayCommand(ClearFields);
        }
        #endregion

        #region Async Methods
        private async Task LoadDataAsync()
        {
            try
            {
                ClearFields();
                _context = new AppDbContext();

                var consumers = await _context.Consumers.ToListAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Consumers = new ObservableCollection<Consumer>(consumers);
                    FilteredConsumers = new ObservableCollection<Consumer>(consumers);
                    OnPropertyChanged(nameof(Consumers));
                    OnPropertyChanged(nameof(FilteredConsumers));
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

            bool result = _isNewRecord ?
                await SaveNewConsumerAsync() :
                await UpdateExistingConsumerAsync();

            if (result)
            {
                FilterConsumers();
                await ShowInfoAsync("Consumer saved successfully.");
            }
        }

        private async Task<bool> SaveNewConsumerAsync()
        {
            if (await _context.Consumers.AnyAsync(c => c.Phone == Phone || c.CommercialRegisterNo == CommercialRegisterNo))
            {
                await ShowWarningAsync("A consumer with this phone number or commercial register already exists.");
                return false;
            }

            var consumer = new Consumer
            {
                Phone = Phone,
                CommercialRegisterNo = CommercialRegisterNo,
                Name = Name,
                Description = Description,
                Whatsapp = Whatsapp,
                Facebook = Facebook,
                TikTok = TikTok,
                X = X,
                Instagram = Instagram,
                Site = Site,
                Notes = Notes,
                Location = Location
            };

            await _context.Consumers.AddAsync(consumer);
            await _context.SaveChangesAsync();

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
            SelectedConsumer.Location = Location;
            SelectedConsumer.Whatsapp = Whatsapp;
            SelectedConsumer.Facebook = Facebook;
            SelectedConsumer.TikTok = TikTok;
            SelectedConsumer.X = X;
            SelectedConsumer.Instagram = Instagram;
            SelectedConsumer.Site = Site;
            SelectedConsumer.Notes = Notes;

            _context.Consumers.Update(SelectedConsumer);
            await _context.SaveChangesAsync();

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
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredConsumers = new ObservableCollection<Consumer>(Consumers);
            }
            else
            {
                var search = SearchText.ToLower();
                FilteredConsumers = new ObservableCollection<Consumer>(
                    Consumers.Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Location) && c.Location.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Whatsapp) && c.Whatsapp.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Facebook) && c.Facebook.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.TikTok) && c.TikTok.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.X) && c.X.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Instagram) && c.Instagram.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Site) && c.Site.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Notes) && c.Notes.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.CommercialRegisterNo) && c.CommercialRegisterNo.ToLower().Contains(search))
                    ));
            }

            OnPropertyChanged(nameof(FilteredConsumers));
        }

        private void AddNewConsumer()
        {
            ClearFields();
            _isNewRecord = true;
            IsFormVisible = true;
        }

        private void EditConsumer()
        {
            IsFormVisible = true;
        }

        private void ClearFields()
        {
            SelectedConsumer = null;
            Phone = CommercialRegisterNo = Name = Description = Location = Whatsapp =
                Facebook = TikTok = X = Instagram = Site = Notes = string.Empty;

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