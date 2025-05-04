using Microsoft.Win32;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;

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
            LoadData();
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

        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }
        public string Location
        {
            get => _location;
            set => SetField(ref _location, value);
        }

        public string Whatsapp
        {
            get => string.IsNullOrEmpty(_whatsapp) ? Phone : _whatsapp;
            set => SetField(ref _whatsapp, value);
        }

        public string Facebook
        {
            get => _facebook;
            set => SetField(ref _facebook, value);
        }

        public string TikTok
        {
            get => _tikTok;
            set => SetField(ref _tikTok, value);
        }

        public string X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        public string Instagram
        {
            get => _instagram;
            set => SetField(ref _instagram, value);
        }

        public string Site
        {
            get => _site;
            set => SetField(ref _site, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetField(ref _phone, value);
        }

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

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand AddNewCommand { get; private set; }
        public RelayCommand ReadExcelCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(SaveConsumer, () => !HasErrors);
            DeleteCommand = new RelayCommand(DeleteConsumer);
            RefreshCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(FilterConsumers);
            AddNewCommand = new RelayCommand(AddNewConsumer);
            ReadExcelCommand = new RelayCommand(ReadExcel);
            EditCommand = new RelayCommand(EditConsumer);
            ClearCommand = new RelayCommand(ClearFields);
        }



        #endregion

        #region Methods

        private void LoadData()
        {
            ClearFields();

            try
            {
                _context = new AppDbContext();

                var consumers = _context.Consumers.ToList();
                Consumers = new ObservableCollection<Consumer>(consumers);
                FilteredConsumers = new ObservableCollection<Consumer>(consumers);

                OnPropertyChanged(nameof(Consumers));
                OnPropertyChanged(nameof(FilteredConsumers));
            }
            catch (Exception ex)
            {
                ShowError($"Error loading data: {ex.Message}");
            }
        }

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

        private void ReadExcel()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var consumers = ReaderHelper.ReadConsumersFromExcel(filePath);

                // Step 1: Validate Excel Data
                var validExcelConsumers = ValidateExcelData(consumers);

                // Step 2: Validate Against Database
                var newConsumers = FilterNewConsumersAgainstDatabase(validExcelConsumers);

                // Step 3: Save to DB
                if (newConsumers.Any())
                {
                    _context.Consumers.AddRange(newConsumers);
                    _context.SaveChanges();
                    new InformationDialog("Consumers saved successfully");
                }
                else
                {
                    new WarningDialog("There are no new consumers!");
                }

                LoadData();
            }
        }
        private List<Consumer> ValidateExcelData(List<Consumer> consumers)
        {
            var validNameConsumers = consumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToList();

            // Ensure unique names
            validNameConsumers = validNameConsumers
                .GroupBy(c => c.Name.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Split phone-based groups
            var uniquePhones = validNameConsumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
                .GroupBy(c => c.Phone.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            var nullPhones = validNameConsumers
                .Where(c => string.IsNullOrWhiteSpace(c.Phone))
                .ToList();

            // Split CRN-based groups
            var uniqueCRNs = uniquePhones
                .Where(c => !string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .GroupBy(c => c.CommercialRegisterNo.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            var nullCRNs = uniquePhones
                .Where(c => string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .ToList();

            // Combine all valid entries
            return uniqueCRNs
                .Concat(nullCRNs)
                .Concat(nullPhones)
                .Distinct()
                .ToList();
        }
        private List<Consumer> FilterNewConsumersAgainstDatabase(List<Consumer> validatedConsumers)
        {
            // Fetch existing values from DB
            var existingNames = _context.Consumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Select(c => c.Name.Trim())
                .ToHashSet();

            var existingPhones = _context.Consumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
                .Select(c => c.Phone.Trim())
                .ToHashSet();

            var existingCRNs = _context.Consumers
                .Where(c => !string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .Select(c => c.CommercialRegisterNo.Trim())
                .ToHashSet();

            // Filter out consumers with duplicate Name, Phone, or CRN
            return validatedConsumers
                .Where(c =>
                {
                    var name = c.Name?.Trim();
                    var phone = c.Phone?.Trim();
                    var crn = c.CommercialRegisterNo?.Trim();

                    bool isNameUnique = !string.IsNullOrWhiteSpace(name) && !existingNames.Contains(name);
                    bool isPhoneUnique = string.IsNullOrWhiteSpace(phone) || !existingPhones.Contains(phone);
                    bool isCrnUnique = string.IsNullOrWhiteSpace(crn) || !existingCRNs.Contains(crn);

                    return isNameUnique && isPhoneUnique && isCrnUnique;
                })
                .ToList();
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

        private void SaveConsumer()
        {
            if (HasErrors)
            {
                ShowWarning("Please fix validation errors before saving.");
                return;
            }

            bool result;

            if (_isNewRecord)
                result = SaveNewConsumer();
            else
                result = UpdateExistingConsumer();

            if (result)
            {
                FilterConsumers();
                ShowInfo("Consumer saved successfully.");
            }

        }

        private bool SaveNewConsumer()
        {
            if (Consumers.Any(c => c.Phone == Phone || c.CommercialRegisterNo == CommercialRegisterNo))
            {
                new WarningDialog("A consumer with this phone number already exists.(1)");
                return false;
            }
            else if (_context.Consumers.Any(c => c.Phone == Phone || c.CommercialRegisterNo == CommercialRegisterNo))
            {
                new WarningDialog("A consumer with this phone number already exists.");
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


            _context.Consumers.Add(consumer);
            _context.SaveChanges();

            Consumers.Add(consumer);
            FilteredConsumers.Add(consumer);

            return true;
        }

        private bool UpdateExistingConsumer()
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
            _context.SaveChanges();

            return true;
        }

        private void DeleteConsumer()
        {
            if (SelectedConsumer == null || _context == null) return;

            var result = new ConfirmationDialog("Are you sure you want to delete this consumer? This action cannot be undone.");
            result.ShowDialog();

            if (!result.IsConfirmed) return;

            try
            {
                _context.Consumers.Remove(SelectedConsumer);
                _context.SaveChanges();

                Consumers.Remove(SelectedConsumer);
                FilteredConsumers.Remove(SelectedConsumer);

                ClearFields();
                ShowInfo("Consumer deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting consumer: {ex.Message}");
            }
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

        private void ShowError(string message) =>
        new ErrorDialog("Error", message);

        private void ShowWarning(string message) =>
        new WarningDialog(message);

        private void ShowInfo(string message) =>
        new InformationDialog(message);

        #endregion
    }
}
